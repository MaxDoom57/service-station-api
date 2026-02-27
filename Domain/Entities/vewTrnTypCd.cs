using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class vewTrnTypCd
    {
        public short TrnTypKy { get; set; }

        [MaxLength(15)]
        public string TrnTypCd { get; set; } = string.Empty;

        [MaxLength(60)]
        public string TrnTypNm { get; set; } = string.Empty;

        public short CKy { get; set; }

        [MaxLength(10)]
        public string? TrnTyp { get; set; }

        public bool fTaxInclusive { get; set; }

        public bool CdF3 { get; set; }
    }
}
