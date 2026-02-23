using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class AccTrn
    {
        [Key]
        public int AccTrnKy { get; set; }
        public int TrnKy { get; set; }
        public short LiNo { get; set; }
        public int AccKy { get; set; }
        public byte fApr { get; set; }
        public short PmtModeKy { get; set; }

        public decimal Amt { get; set; }
        public decimal FrnAmt { get; set; }

        public short CrnKy { get; set; }
        public float ExRate { get; set; }

        public bool fChqDet { get; set; }
        public short BUKy { get; set; }

        public string? Des { get; set; }

        public short AnlTyp1Ky { get; set; }
        public short AnlTyp2Ky { get; set; }
        public short AnlTyp3Ky { get; set; }
        public short AnlTyp4Ky { get; set; }

        public int AccTrnTrfLnkKy { get; set; }
        public bool fVirtAccTrn { get; set; }

        public string? Status { get; set; }
        public short SKy { get; set; }

        public int EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }

        public int UpdUsrKy { get; set; }
        public int AdrKy { get; set; }
    }
}
