using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class BayReservation
    {
        [Key]
        public int ResKy { get; set; }

        public int BayKy { get; set; } // FK to Bay
        
        public int? ReservationMasKy { get; set; } // FK to ReservationMas (optional)
        
        public int? VehicleKy { get; set; } // FK to Vehicle

        public DateTime FromDtm { get; set; } // Start Time
        public DateTime ToDtm { get; set; }   // End Time

        [MaxLength(20)]
        public string ResType { get; set; } // 'Online', 'Physical'

        // Status: 'Pending', 'Approved', 'Cancelled', 'Completed'
        [MaxLength(20)]
        public string ResStatus { get; set; } 

        public bool fInAct { get; set; } // Soft Delete

        public int EntUsrKy { get; set; }
        public DateTime EntDtm { get; set; }
        public int CKy { get; set; }
    }
}
