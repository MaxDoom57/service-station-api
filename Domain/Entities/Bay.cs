using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Bay
    {
        [Key]
        public int BayKy { get; set; } // Primary Key

        [Required]
        [MaxLength(20)]
        public string BayCd { get; set; } // Bay Code

        [Required]
        [MaxLength(60)]
        public string BayNm { get; set; } // Bay Name

        public bool fInAct { get; set; } // Is Active / Deleted

        public bool IsReservationAvailable { get; set; } // Reservation Available Or Not

        [MaxLength(200)]
        public string? Description { get; set; }

        public int EntUsrKy { get; set; }
        public DateTime EntDtm { get; set; }

        public short CKy { get; set; } // Company Key
    }
}
