using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class TrnNoLst
    {
        [Key]
        public int TrnNoLstKy { get; set; }
        public bool fInAct { get; set; }
        public string? Status { get; set; }
        public short CKy { get; set; }
        public short SKy { get; set; }
        public int Period { get; set; }
        public string OurCd { get; set; }
        public short CdKy { get; set; }
        public int LstTrnNo { get; set; }
        public string? LstDocNo { get; set; }
    }
}
