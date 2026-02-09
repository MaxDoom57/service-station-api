using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Item
    {
        public int CKy { get; set; }
        public int ItmKy { get; set; }
        public short ItmTypKy { get; set; }
        public string? BseItmCd { get; set; }
        public string ItmCd { get; set; }
        public string? PartNo { get; set; }
        public string ItmNm { get; set; }
        public string ItmTypCd { get; set; }
        public string ItmTyp { get; set; }
        public short? ItmCat1Ky { get; set; }
        public short? ItmCat2Ky { get; set; }
        public short? ItmCat3Ky { get; set; }
        public decimal? CosPri { get; set; }
        public decimal? SlsPri { get; set; }
        public decimal? SlsPri2 { get; set; }
        public short? UnitKy { get; set; }
        public string Unit { get; set; }
        public short? ItmPrp1Ky { get; set; }
        public short? ItmPrp2Ky { get; set; }
        public short? BUKy { get; set; }
        public float? Wrnty { get; set; }
        public bool? fSrlNo { get; set; }
        public string? ItmRem { get; set; }
        public short? ItmCat4Ky { get; set; }

    }
}

