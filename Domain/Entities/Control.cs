using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Control
    {
        [Key]
        public int ConKy { get; set; }

        public short CKy { get; set; }

        public string ConCd { get; set; } = string.Empty;

        public bool fInAct { get; set; }

        public byte fApr { get; set; }

        public string? ConNm { get; set; }

        public string? ConDes { get; set; }

        public int? ObjKy { get; set; }

        public bool fUsrAcs { get; set; }

        public bool fCCAcs { get; set; }

        public bool fDevAcs { get; set; }

        public bool fCol { get; set; }

        public bool fNo { get; set; }

        public int? MaxCdLen { get; set; }

        public bool ConF1 { get; set; }
        public bool ConF2 { get; set; }
        public bool ConF3 { get; set; }
        public bool ConF4 { get; set; }
        public bool ConF5 { get; set; }
        public bool ConF6 { get; set; }
        public bool ConF7 { get; set; }
        public bool ConF8 { get; set; }
        public bool ConF9 { get; set; }
        public bool ConF10 { get; set; }

        public int ConInt1 { get; set; }

        public int? PrntKy { get; set; }

        public string? Status { get; set; }

        public short SKy { get; set; }

        public int? EntUsrKy { get; set; }

        public DateTime? EntDtm { get; set; }
    }
}
