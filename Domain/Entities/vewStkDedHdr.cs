using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class vewStkDedHdr
    {
        public short LocKy { get; set; }
        public DateTime? TrnDt { get; set; }

        [MaxLength(60)]
        public string? Des { get; set; }

        public int TrnKy { get; set; }
        public int TrnNo { get; set; }
        public short CKy { get; set; }
    }
}
