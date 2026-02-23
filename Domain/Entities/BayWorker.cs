using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class BayWorker
    {
        [Key]
        public int BayWorkerKy { get; set; }

        public int BayKy { get; set; } // FK to Bay
        
        public int UsrKy { get; set; } // FK to UsrMas (Worker)

        public bool fInAct { get; set; } // Soft Delete / Inactive

        public string? Remarks { get; set; }

        public int EntUsrKy { get; set; }
        public DateTime EntDtm { get; set; }
        public int CKy { get; set; }
    }
}
