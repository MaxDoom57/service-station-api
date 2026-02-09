using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Core.Entities.Reports
{
    [Table("vewSlsDtlsRpt")]
    public class VewSlsDtlsRpt
    {
        [Key]
        [Column(Order = 0)]
        public int ItmTrnKy { get; set; }

        public int ItmKy { get; set; }

        [MaxLength(25)]
        public string ItmCd { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? ItmNm { get; set; }

        [MaxLength(15)]
        public string? Unit { get; set; }

        [Column(TypeName = "money")]
        public decimal CosPri { get; set; }

        [Column(TypeName = "money")]
        public decimal SlsPri { get; set; }

        [Column(TypeName = "money")]
        public decimal? TrnPri { get; set; }

        public float DisPer { get; set; }

        public double? Qty { get; set; }

        public int UpDt { get; set; }

        public short LiNo { get; set; }

        public int TrnNo { get; set; }

        public int RepAdrKy { get; set; }

        [MaxLength(15)]
        public string? SlsRepAdrCd { get; set; }

        [MaxLength(60)]
        public string? SlsRepAdrNm { get; set; }

        [Column(TypeName = "money")]
        public decimal DisAmt { get; set; }

        public DateTime TrnDt { get; set; }

        [MaxLength(10)]
        public string? OurCd { get; set; }

        public int AccKy { get; set; }

        [MaxLength(15)]
        public string? AccCd { get; set; }

        [MaxLength(60)]
        public string? AccNm { get; set; }
    }
}
