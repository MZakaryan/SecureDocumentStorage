using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SecureDocumentStorage.Models.DocumentViewModels
{
    public class AddEditDocumentViewModel
    {
        [Required]
        public string EncryptedDocument { get; set; }
        public bool IsPublic { get; set; }
        [Required]
        public string FileName { get; set; }
    }
}
