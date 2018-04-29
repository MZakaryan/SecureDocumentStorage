using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SecureDocumentStorage.Models.DocumentViewModels
{
    public class DocumentInfoViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; }

        [DisplayName("This document is public?")]
        public bool IsPublic { get; set; }
    }
}
