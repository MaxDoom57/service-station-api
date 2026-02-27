using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ItmTrn
    {
        [Key]
        public int ItmTrnKy { get; set; }
        public int TrnKy { get; set; }
        public short LiNo { get; set; }
        public short fApr { get; set; }
        public short CKy { get; set; }
        public int ItmTrnTrfLnkKy { get; set; }
        public int RefItmTrnKy { get; set; }
        public bool fVirtItmTrn { get; set; }
        public int ItmKy { get; set; }
        public string? Des { get; set; }
        public string? ItmTrnRem { get; set; }
        public short LocKy { get; set; }
        public int CdKy1 { get; set; }

        public double Qty { get; set; }
        public double Qty2 { get; set; }
        public double CnvFct { get; set; }
        public double IntQty { get; set; }
        public int IntUnitKy { get; set; }
        public double FrctQty { get; set; }
        public double FrctFctr { get; set; }
        public short FrctUnitKy { get; set; }
        public double BulkQty { get; set; }
        public double BulkFctr { get; set; }
        public short BulkUnitKy { get; set; }

        public short BUKy { get; set; }

        public decimal DisAmt { get; set; }
        public float DisPer { get; set; }
        public float HdrDisPer { get; set; }
        public float LineDisPer { get; set; }
        public float ComisPer { get; set; }

        public int OrdKy { get; set; }
        public int OrdDetKy { get; set; }

        public decimal CosPri { get; set; }
        public decimal SlsPri { get; set; }
        public decimal SlsPri2 { get; set; }
        public decimal TrnPri { get; set; }

        public decimal Amt1 { get; set; }
        public decimal Amt2 { get; set; }
        public decimal Amt3 { get; set; }
        public decimal Amt4 { get; set; }

        public short AcsLvlKy { get; set; }

        public bool fQty { get; set; }
        public byte fPri { get; set; }
        public bool fVal { get; set; }
        public bool fDisplay { get; set; }
        public bool fNoPrnPri { get; set; }

        public short SKy { get; set; }

        public int EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }
    }
}
