using SecureDocumentStorage.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SecureDocumentStorage.Data.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        [ForeignKey(nameof(ApplicationUser))]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        public bool Deleted { get; set; }
        [Required]
        public bool IsPublic { get; set; }
    }
}
