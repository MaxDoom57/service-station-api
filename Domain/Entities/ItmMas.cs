using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ItmMas
    {
        [Key]
        public int ItmKy { get; set; }
        public int CKy { get; set; }

        [MaxLength(25)]
        public string ItmCd { get; set; }

        public bool fInAct { get; set; }
        public bool fApr { get; set; }
        public bool fObs { get; set; }

        public short ItmTypKy { get; set; }

        [MaxLength(10)]
        public string ItmTyp { get; set; }

        [MaxLength(60)]
        public string? PartNo { get; set; }

        [MaxLength(60)]
        public string? ItmNm { get; set; }

        [MaxLength(15)]
        public string? BseItmCd { get; set; }

        [MaxLength(60)]
        public string? Des { get; set; }

        public short LocKy { get; set; }
        public short ItmCat1Ky { get; set; }
        public short ItmCat2Ky { get; set; }
        public short ItmCat3Ky { get; set; }
        public short ItmCat4Ky { get; set; }
        public short ItmPriCatKy { get; set; }
        public short ItmPrp1Ky { get; set; }
        public short ItmPrp2Ky { get; set; }

        [MaxLength(30)]
        public string? Make { get; set; }

        [MaxLength(30)]
        public string? Model { get; set; }

        [Column(TypeName = "real")]
        public float Wrnty { get; set; }

        public double? AvrWt { get; set; }

        public bool fCtrlItm { get; set; }
        public bool fSrlNo { get; set; }
        public bool fChkStk { get; set; }

        public short BUKy { get; set; }
        public double? FrctFctr { get; set; }
        public short? FrctUnitKy { get; set; }
        public short UnitKy { get; set; }
        public double? IntrFctr { get; set; }
        public int? IntrUnitKy { get; set; }
        public double? BulkFctr { get; set; }
        public short? BulkUnitKy { get; set; }
        public double? ReOrdLvl { get; set; }
        public double? ReOrdQty { get; set; }
        public double? OnOrdQty { get; set; }
        public double? ResrvQty { get; set; }

        [Column(TypeName = "real")]
        public float DisPer { get; set; }

        [Column(TypeName = "money")]
        public decimal CosPri { get; set; }

        [Column(TypeName = "money")]
        public decimal SlsPri { get; set; }

        [Column(TypeName = "money")]
        public decimal SlsPri2 { get; set; }

        [Column(TypeName = "real")]
        public float DisAmt { get; set; }

        public double Qty { get; set; }
        public double? GrsWt { get; set; }
        public double? NetWt { get; set; }

        [Column(TypeName = "text")]
        public string? ItmRem { get; set; }

        public int Rac1Ky { get; set; }
        public int Rac2Ky { get; set; }
        public int SupAdrKy { get; set; }

        [MaxLength(2)]
        public string? Status { get; set; }

        public int? EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }

        [Column(TypeName = "real")]
        public float PrftMrgn { get; set; }
    }
}
