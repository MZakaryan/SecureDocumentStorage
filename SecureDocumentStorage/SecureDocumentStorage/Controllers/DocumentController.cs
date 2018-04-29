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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signingManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHostingEnvironment _appEnvironment;

        public DocumentController(UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IHostingEnvironment appEnvironment,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _appEnvironment = appEnvironment;
            _signingManager = signInManager;
        }
        
        public async Task<IActionResult> Index()
        {
            var docs = _dbContext.Documents
                                 .Include(d => d.User)
                                 .Where(d => d.Deleted == false);
            if (!_signingManager.IsSignedIn(User))
            {
                docs = docs.Where(d => d.IsPublic == true);
            }
            else
            {
                ApplicationUser currentUser =
                await _userManager.GetUserAsync(HttpContext.User);
                docs = docs.Where(d => d.UserId == currentUser.Id);
            }
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

        [Authorize]
        public IActionResult Upload()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadDocument(AddEditDocumentViewModel model)
        {
            if (model.Document == null || model.Document.Length == 0)
            {
                throw new Exception("boom");
            }

            string documentName = "document_" +
                                  Guid.NewGuid();

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

            using (var stream = new FileStream(documentPath, FileMode.Create))
            {
                await model.Document.CopyToAsync(stream);
            }

            Document document = new Document()
            {
                Date = DateTime.Now,
                Name = model.Document.FileName,
                ContentType = model.Document.ContentType,
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
                                      .Where(d => d.Id == id)
                                      .SingleAsync();

            var path = Path.Combine(
                           _appEnvironment.WebRootPath,
                           "files",
                           "documents",
                           doc.User.UserName + "_" +
                           doc.UserId);
            
            return DownloadFile(path, 
                                doc.Path, 
                                doc.Name, 
                                doc.ContentType);
        }

        private FileResult DownloadFile(string folderPath, 
                                        string filePath, 
                                        string fileName,
                                        string contentType)
        {
            IFileProvider provider = new PhysicalFileProvider(folderPath);
            IFileInfo fileInfo = provider.GetFileInfo(filePath);
            var readStream = fileInfo.CreateReadStream();
            return File(readStream, contentType, fileName);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _dbContext.Documents
                                      .Include(d => d.User)
                                      .Where(d => d.Id == id)
                                      .SingleAsync();
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