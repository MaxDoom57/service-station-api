using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reports
{
    public class vewReOrdDet
    {
        public bool fInAct { get; set; }

        public int CKy { get; set; }

        public int ItmKy { get; set; }
        public string ItmCd { get; set; } = null!;
        public string? PartNo { get; set; }
        public string? ItmNm { get; set; }

        public float ReOrdLvl { get; set; }
        public float ReOrdQty { get; set; }

        public decimal CosPri { get; set; }

        public string ItmTyp { get; set; } = null!;

        public float ItmLocQty { get; set; }

        public short ItmCat1Ky { get; set; }
        public short ItmCat2Ky { get; set; }

        public float ItmTrnQty { get; set; }

        public DateTime TrnDt { get; set; }

        public string? OurCd { get; set; }

        public int TrnNo { get; set; }
    }
}
