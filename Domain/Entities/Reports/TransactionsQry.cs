using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Core.Entities.Reports
{
    [Table("TransactionsQry")]
    public class TransactionsQry
    {
        public short CKy { get; set; }

        public int AccKy { get; set; }

        public DateTime TrnDt { get; set; }

        public int TrnKy { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string Code { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string? DocNo { get; set; }

        public int TrnNo { get; set; }

        public int RepAdrKy { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? RepAdrCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? RepAdrNm { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? AccCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? AccNm { get; set; }

        [Column(TypeName = "varchar(120)")]
        public string? Des { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amt { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string? OurCd { get; set; }

        public short? BUKy { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? BUCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? BUNm { get; set; }

        public short LocKy { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? LocCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? LocNm { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? YurRef { get; set; }

        public short LiNo { get; set; }

        public short TrnTypKy { get; set; }

        [Column(TypeName = "varchar(120)")]
        public string? AccTrnDesc { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? CrnCd { get; set; }

        [Column(TypeName = "money")]
        public decimal? FrnAmt { get; set; }

        public float ExRate { get; set; }

        public bool fInAct { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? CusNm { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? CNm { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string? AccTyp { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? ChqNo { get; set; }

        public int AccTrnKy { get; set; }

        public short AccTypKy { get; set; }
    }
}
