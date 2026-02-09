using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class vewPURRTNHdr
    {
        public int TrnKy { get; set; }

        public DateTime TrnDt { get; set; }

        [MaxLength(15)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? YurRef { get; set; }

        [MaxLength(60)]
        public string? AdrNm { get; set; }

        [MaxLength(15)]
        public string? AdrCd { get; set; }

        public short LocKy { get; set; }

        public int AdrKy { get; set; }

        public int AccKy { get; set; }

        public int PurAccKy { get; set; }

        [MaxLength(15)]
        public string? PurAccCd { get; set; }

        public int AccTrnKy { get; set; }

        [MaxLength(15)]
        public string? AccCd { get; set; }

        [MaxLength(60)]
        public string? AccNm { get; set; }

        public short PmtTrmKy { get; set; }

        [MaxLength(15)]
        public string? DocNo { get; set; }
    }
}
