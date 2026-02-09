using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class vewGRNDtls
    {
        public int ItmKy { get; set; }

        [MaxLength(25)]
        public string ItmCd { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? ItmNm { get; set; }

        [MaxLength(15)]
        public string? Unit { get; set; }

        [Column(TypeName = "money")]
        public decimal? CosPri { get; set; }

        [Column(TypeName = "money")]
        public decimal? TrnPri { get; set; }

        [Column(TypeName = "money")]
        public decimal SlsPri { get; set; }

        public double Qty { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt1 { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt2 { get; set; }

        public int ItmTrnKy { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt3 { get; set; }

        public int TrnKy { get; set; }

        public short CKy { get; set; }

        [MaxLength(15)]
        public string? PurTrmCd { get; set; }

        public double Qty2 { get; set; }

        [MaxLength(60)]
        public string? BatchNo { get; set; }

        public DateTime? ExpirDt { get; set; }
    }
}
