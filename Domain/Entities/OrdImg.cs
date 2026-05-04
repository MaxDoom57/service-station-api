using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class OrdImg
    {
        [Key]
        public int ImageKy { get; set; }
        
        public int OrdKy { get; set; }
        public int ServiceOrdKy { get; set; }
        
        [MaxLength(500)]
        public string ImageUrl { get; set; }
        
        [MaxLength(255)]
        public string PublicId { get; set; }
        
        [MaxLength(50)]
        public string ImageType { get; set; } // e.g., 'Signature', 'Vehicle'
        
        public bool fInAct { get; set; }
        
        public int EntUsrKy { get; set; }
        public DateTime EntDtm { get; set; }
    }
}
