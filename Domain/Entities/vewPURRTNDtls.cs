using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class vewPURRTNDtls
    {
        public int ItmKy { get; set; }

        [MaxLength(15)]
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

        [Column(TypeName = "money")]
        public decimal Amt1 { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt2 { get; set; }

        public int ItmTrnKy { get; set; }

        public double Qty { get; set; }

        public int TrnKy { get; set; }
    }
}
