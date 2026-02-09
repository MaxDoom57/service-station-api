using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class BayControl
    {
        [Key]
        public int BayControlKy { get; set; }

        public int BayKy { get; set; } // FK to Bay
        
        [MaxLength(20)]
        public string BayCd { get; set; } 

        public bool IsBayOccupied { get; set; } // 1 = Occupied, 0 = Available

        public int? CurrentVehicleKy { get; set; } // Active Vehicle on Bay
        
        [MaxLength(100)]
        public string? CurrentActivity { get; set; } // e.g. "Oil Change", "Washing"

        public DateTime? EstimatedFinishDtm { get; set; } // Estimated Time for current service

        public DateTime LastUpdDtm { get; set; }
        public int CKy { get; set; }
    }
}
