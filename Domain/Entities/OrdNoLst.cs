using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class OrdNoLst
    {
        [Key]
        [Column("OrdNoLstKy")]
        public int OrdNoLstKy { get; set; }

        public short CKy { get; set; }

        [MaxLength(20)]
        public string OurCd { get; set; }

        public int LstOrdNo { get; set; }

        public bool fInAct { get; set; }

        [MaxLength(10)]
        public string? Status { get; set; }

        public short SKy { get; set; }

        public int? Period { get; set; }

        public short CdKy { get; set; }
    }
}
