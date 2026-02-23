using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class vewGRNHdr
    {
        public int TrnNo { get; set; }

        public DateTime TrnDt { get; set; }

        [MaxLength(15)]
        public string? AccCd { get; set; }

        [MaxLength(60)]
        public string? AccNm { get; set; }

        [MaxLength(10)]
        public string? AccTyp { get; set; }

        [MaxLength(15)]
        public string? Code { get; set; }

        [MaxLength(15)]
        public string? YurRef { get; set; }

        [MaxLength(60)]
        public string? Des { get; set; }

        public int AccKy { get; set; }

        public int Adrky { get; set; }

        public int TrnKy { get; set; }

        public short LocKy { get; set; }

        public int? PurAccKy { get; set; }

        [MaxLength(15)]
        public string? PurAccCd { get; set; }

        [MaxLength(60)]
        public string? PurAccNm { get; set; }

        public float ComisPer { get; set; }

        public short PmtTrmKy { get; set; }

        public bool fTax { get; set; }

        [MaxLength(15)]
        public string? DocNo { get; set; }
    }
}
