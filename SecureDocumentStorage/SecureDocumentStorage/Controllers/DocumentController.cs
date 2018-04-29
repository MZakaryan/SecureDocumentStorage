using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureDocumentStorage.Models;
using SecureDocumentStorage.Models.DocumentViewModels;

namespace SecureDocumentStorage.Controllers
{
    public class DocumentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(AddEditDocumentViewModel model)
        {
            if (model.Document == null || model.Document.Length == 0)
            {
                throw new Exception("boom");
            }

            string documentName = "document_" + 
                                  Guid.NewGuid() + "_" +
                                  model.Document.FileName;

            ApplicationUser currentUser = 
                await _userManager.GetUserAsync(HttpContext.User);

            string path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
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

            return RedirectToAction(nameof(Index));
        }

    }
}