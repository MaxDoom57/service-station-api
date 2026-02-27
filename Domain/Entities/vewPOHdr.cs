using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class vewPOHdr
    {
        [MaxLength(15)]
        public string? OrdFrq { get; set; }

        public int? OrdNo { get; set; }

        [MaxLength(15)]
        public string? DocNo { get; set; }

        public DateTime? OrdDt { get; set; }

        [MaxLength(60)]
        public string? Des { get; set; }

        [MaxLength(60)]
        public string? AdrNm { get; set; }

        [MaxLength(15)]
        public string? AdrCd { get; set; }

        public int OrdKy { get; set; }

        public short OrdTypKy { get; set; }

        [MaxLength(10)]
        public string? OrdTyp { get; set; }

        public short CKy { get; set; }

        public int? AccKy { get; set; }

        public int AdrKy { get; set; }

        [MaxLength(15)]
        public string? AccCd { get; set; }

        [MaxLength(60)]
        public string? AccNm { get; set; }
    }
}
