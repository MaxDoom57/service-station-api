using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class vewOrdNo
    {
        public short CKy { get; set; }

        public int OrdKy { get; set; }

        public int? OrdNo { get; set; }

        [MaxLength(15)]
        public string? DocNo { get; set; }

        [MaxLength(10)]
        public string? OrdTyp { get; set; }

        public short OrdTypKy { get; set; }

        [MaxLength(10)]
        public string? OurCd { get; set; }

        [MaxLength(15)]
        public string? OrdCat1Cd { get; set; }

        [MaxLength(60)]
        public string? OrdCat1Nm { get; set; }

        public bool fDefault { get; set; }

        public int OrdRelKy { get; set; }
    }
}
