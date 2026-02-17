using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reports
{
    public class vewItmMasVsf
    {
        public int Edited { get; set; }

        public int ItmKy { get; set; }
        
        public string? Des { get; set; }

        public string ItmCd { get; set; } = null!;

        public string? ItmNm { get; set; }

        public string? PartNo { get; set; }

        public string? Unit { get; set; }

        public string? ItmCat1 { get; set; }
        public string? ItmCat2 { get; set; }
        public string? ItmCat3 { get; set; }
        public string? ItmCat4 { get; set; }

        public decimal CosPri { get; set; }
        public decimal SlsPri { get; set; }

        public bool fSrlNo { get; set; }

        public float Wrnty { get; set; }

        public bool fInAct { get; set; }

        public string? ItmCat1Cd { get; set; }

        public string? ItmPriCatCd { get; set; }

        public short ItmTypKy { get; set; }

        public short BUKy { get; set; }

        public string? BUCd { get; set; }

        public string? ItmRem { get; set; }

        public double? ReOrdLvl { get; set; }

        public double? ReOrdQty { get; set; }

        public decimal SlsPri2 { get; set; }

        //public float PrftMrgn { get; set; }

        public int Rac1Ky { get; set; }
        public int Rac2Ky { get; set; }

        public string? Rac1Cd { get; set; }
        public string? Rac1Nm { get; set; }

        public string? Rac2Cd { get; set; }
        public string? Rac2Nm { get; set; }

        public bool fChkStk { get; set; }

        //public float DisPer { get; set; }

        //public short ItmCat1Ky { get; set; }
        //public short ItmCat2Ky { get; set; }
        //public short ItmCat3Ky { get; set; }
    }
}
