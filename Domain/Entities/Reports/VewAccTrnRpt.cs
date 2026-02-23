using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Core.Entities.Reports
{
    [Table("vewAccTrnRpt")]
    public class VewAccTrnRpt
    {
        public DateTime TrnDt { get; set; }

        public int TrnKy { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string TrnType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string? DocNo { get; set; }

        public int TrnNo { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string? YurRef { get; set; }

        public short TrnTypKy { get; set; }

        public bool fInAct { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? ChqNo { get; set; }

        [Column(TypeName = "varchar(120)")]
        public string? Des { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amt { get; set; }

        public short LiNo { get; set; }

        public int AccKy { get; set; }

        public short CKy { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? BUCd { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? AnlTyp1Cd { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? AnlTyp2Cd { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? RepAdrCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? RepAdrNm { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? LocCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? LocNm { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? AccCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? AccNm { get; set; }

        public short AccTypKy { get; set; }

        public short LocKy { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? CrnCd { get; set; }

        [Column(TypeName = "money")]
        public decimal? FrnAmt { get; set; }

        public float ExRate { get; set; }

        public short AnlTyp1Ky { get; set; }

        public short BUKy { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? BUNm { get; set; }
    }
}
