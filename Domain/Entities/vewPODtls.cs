using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class vewPODtls
    {
        public int? ItmKy { get; set; }

        [MaxLength(25)]
        public string ItmCd { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? ItmNm { get; set; }

        public double? OrdQty { get; set; }

        [MaxLength(15)]
        public string? Unit { get; set; }

        [Column(TypeName = "money")]
        public decimal? CosPri { get; set; }

        public DateTime? ReqDt { get; set; }

        [MaxLength(120)]
        public string? Des { get; set; }

        public int OrdDetKy { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt1 { get; set; }

        public int OrdKy { get; set; }

        public short CKy { get; set; }

        public int? OrdNo { get; set; }

        public short OrdTypKy { get; set; }

        [Column(TypeName = "money")]
        public decimal SlsPri { get; set; }
    }
}
