using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SecureDocumentStorage.Models.DocumentViewModels
{
    public class AddEditDocumentViewModel
    {
        [Required]
        public IFormFile Document { get; set; }
    }
}
