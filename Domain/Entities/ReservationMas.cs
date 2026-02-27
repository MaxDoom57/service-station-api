using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ReservationMas
    {
        [Key]
        public int ResKy { get; set; }

        public int VehicleKy { get; set; } // FK to Vehicle

        public int PackageKy { get; set; } // FK to CdMas (Package)

        [MaxLength(20)]
        public string ResStatus { get; set; } // 'Pending', 'Approved', 'Cancelled', 'Completed'

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public bool fInAct { get; set; }

        public int EntUsrKy { get; set; }
        public DateTime EntDtm { get; set; }
        public int CKy { get; set; }
    }
}
