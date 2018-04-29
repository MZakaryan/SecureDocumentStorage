using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SecureDocumentStorage.Models.DocumentViewModels
{
    public class AddEditDocumentViewModel
    {
        [Required]
        public IFormFile Document { get; set; }
        
        public bool IsPublic { get; set; }
    }
}
