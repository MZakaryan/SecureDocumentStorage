using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SecureDocumentStorage.Data;
using SecureDocumentStorage.Data.Entities;
using SecureDocumentStorage.Models;
using SecureDocumentStorage.Models.DocumentViewModels;

namespace SecureDocumentStorage.Controllers
{
    public class DocumentController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signingManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHostingEnvironment _appEnvironment;

        public DocumentController(SignInManager<ApplicationUser> signingManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IHostingEnvironment appEnvironment)
        {
            _signingManager = signingManager;
            _userManager = userManager;
            _dbContext = dbContext;
            _appEnvironment = appEnvironment;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ApplicationUser currentUser =
                await _userManager.GetUserAsync(HttpContext.User);
            var docs = _dbContext.Documents
                                 .Include(d => d.User)
                                 .Where(d => d.Deleted == false)
                                 .Where(d => d.UserId == currentUser.Id);
            List<DocumentInfoViewModel> model =
                new List<DocumentInfoViewModel>();

            foreach (Document item in docs)
            {
                model.Add(new DocumentInfoViewModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    UserName = item.User.UserName,
                    Date = item.Date,
                    IsPublic = item.IsPublic,
                });
            }

            return View(model);
        }

        public IActionResult PublicDocuments()
        {
            var docs = _dbContext.Documents
                                 .Include(d => d.User)
                                 .Where(d => d.Deleted == false)
                                 .Where(d => d.IsPublic == true);

            List<DocumentInfoViewModel> model =
                new List<DocumentInfoViewModel>();

            foreach (Document item in docs)
            {
                model.Add(new DocumentInfoViewModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    UserName = item.User.UserName,
                    Date = item.Date,
                });
            }

            return View(model);
        }

        [Authorize]
        public IActionResult Upload()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadDocument(AddEditDocumentViewModel model)
        {
            if (string.IsNullOrEmpty(model.EncryptedDocument))
            {
                throw new Exception("boom");
            }

            string documentName = "document_" +
                                  Guid.NewGuid() +
                                  ".txt";

            ApplicationUser currentUser =
                await _userManager.GetUserAsync(HttpContext.User);

            string path = Path.Combine(
                _appEnvironment.WebRootPath,
                "files",
                "documents",
                currentUser.UserName + "_" +
                currentUser.Id);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string documentPath = Path.Combine(path, documentName);

            using (var stream = new FileStream(documentPath, FileMode.Create)) { }

            using (StreamWriter writer = new StreamWriter(documentPath))
            {
                await writer.WriteAsync(model.EncryptedDocument);
            }

            Document document = new Document()
            {
                Date = DateTime.Now,
                Name = model.FileName,
                Path = documentName,
                UserId = currentUser.Id,
                IsPublic = model.IsPublic,
            };

            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(int id)
        {
            var doc = await _dbContext.Documents
                                .Include(d => d.User)
                                .Where(d => !d.Deleted)
                                .Where(d => d.Id == id)
                                .SingleAsync();

            if (!doc.IsPublic)
            {
                if (_signingManager.IsSignedIn(User))
                {
                    ApplicationUser currentUser =
                    await _userManager.GetUserAsync(HttpContext.User);
                    if (doc.UserId != currentUser.Id)
                    {
                        return RedirectToAction(nameof(PublicDocuments));
                    }
                }
                else
                {
                    return RedirectToAction(nameof(PublicDocuments));
                }
            }

            var path = Path.Combine(
                       _appEnvironment.WebRootPath,
                       "files",
                       "documents",
                       doc.User.UserName + "_" +
                       doc.UserId,
                       doc.Path);

            string encDoc;

            using (StreamReader reader = new StreamReader(path))
            {
                encDoc = await reader.ReadToEndAsync();
            }

            return Json(new
            {
                encryptedDocument = encDoc,
                fileName = doc.Name
            });
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _dbContext.Documents
                                      .Include(d => d.User)
                                      .Where(d => d.Id == id)
                                      .SingleAsync();
            if (_signingManager.IsSignedIn(User))
            {
                ApplicationUser currentUser =
                await _userManager.GetUserAsync(HttpContext.User);
                if (doc.UserId != currentUser.Id)
                {
                    return RedirectToAction(nameof(PublicDocuments));
                }
            }
            doc.Deleted = true;
            _dbContext.Entry(doc).State = EntityState.Modified;
            var path = Path.Combine(
                           _appEnvironment.WebRootPath,
                           "files",
                           "documents",
                           doc.User.UserName + "_" +
                           doc.UserId,
                           doc.Path);
            System.IO.File.Delete(path);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}