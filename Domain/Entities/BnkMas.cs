using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class BnkMas
    {
        [Key]
        public short BnkKy { get; set; }

        public string BnkCd { get; set; } = string.Empty;

        public bool fInAct { get; set; }

        public byte fApr { get; set; }

        public string? BnkNm { get; set; }

        public string? DepSlipFmt { get; set; }

        public string? ChkPrnFmt { get; set; }

        public string? Status { get; set; }

        public short SKy { get; set; }

        public int? EntUsrKy { get; set; }

        public DateTime? EntDtm { get; set; }
    }
}
