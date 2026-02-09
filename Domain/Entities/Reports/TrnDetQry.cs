using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Core.Entities.Reports
{
    [Table("TrnDetQry")]
    public class TrnDetQry
    {
        public DateTime TrnDt { get; set; }

        public int TrnNo { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string TrnTyp { get; set; } = string.Empty;

        [Column(TypeName = "varchar(60)")]
        public string CdNm { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string? DocNo { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string? YurRef { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? AdrNm { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string ItmCd { get; set; } = string.Empty;

        [Column(TypeName = "varchar(60)")]
        public string? ItmNm { get; set; }

        public double Qty { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? Unit { get; set; }

        [Column(TypeName = "money")]
        public decimal? TrnPri { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt1 { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt2 { get; set; }

        [Column(TypeName = "money")]
        public decimal DisAmt { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt3 { get; set; }

        public short? ItmCat1Ky { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? ItmCat1Cd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? ItmCat1Nm { get; set; }

        public double Qty2 { get; set; }

        public float Weight { get; set; }

        public short? BUKy { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? BUCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? BUNm { get; set; }

        public short LocKy { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? UsrId { get; set; }
    }
}
