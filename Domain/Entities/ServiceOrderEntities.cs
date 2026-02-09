using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ServiceOrder
    {
        [Key]
        public int ServiceOrdKy { get; set; }

        [MaxLength(20)]
        public string ServiceOrdNo { get; set; } 

        public int VehicleKy { get; set; }
        public int AccKy { get; set; } // Customer Account
        public int PackageKy { get; set; }
        public int BayKy { get; set; }

        public float? CurrentMileage { get; set; }
        
        [MaxLength(500)]
        public string? DamageNote { get; set; }
        
        [MaxLength(500)]
        public string? Remarks { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } // Wait, Ongoing, Finish

        public bool fInAct { get; set; }
        public int EntUsrKy { get; set; }
        public DateTime EntDtm { get; set; }
        public int CKy { get; set; }
    }

    public class ServiceOrderDetail
    {
        [Key]
        public int ServiceOrdDetKy { get; set; }

        public int ServiceOrdKy { get; set; } // FK

        public int? ItemKy { get; set; } // Nullable if ad-hoc

        [MaxLength(200)]
        public string ItemName { get; set; }

        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string EstimatedTime { get; set; } // e.g., "30 mins"

        // Status Stages (0 or 1 as requested)
        public int StatusWait { get; set; }
        public int StatusInProgress { get; set; }
        public int StatusFinish { get; set; }

        public bool IsApproved { get; set; } // For added items

        public int EntUsrKy { get; set; }
        public DateTime EntDtm { get; set; }
    }

    public class ServiceOrderApproval
    {
        [Key]
        public int ApprovalKy { get; set; }

        public int ServiceOrdDetKy { get; set; }

        [MaxLength(100)]
        public string CustName { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }

        [MaxLength(100)]
        public string Device { get; set; }

        public DateTime ApprovedDtm { get; set; }
    }
}
