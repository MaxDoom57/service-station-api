using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Core.Entities.Reports
{
    [Table("vewChqRtnDet")]
    public class VewChqRtnDet
    {
        public int TrnKy { get; set; }

        public short CKy { get; set; }

        public DateTime TrnDt { get; set; }

        public int TrnNo { get; set; }

        public bool fInAct { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string? OurCd { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? DocNo { get; set; }

        public short TrnTypKy { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? Des { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? ChqNo { get; set; }

        public short LiNo { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amt { get; set; }

        public int AccKy { get; set; }
    }
}
