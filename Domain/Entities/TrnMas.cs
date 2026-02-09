using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class TrnMas
    {
        [Key]
        public int TrnKy { get; set; }
        public short CKy { get; set; }
        public DateTime TrnDt { get; set; }
        public short TrnTypKy { get; set; }
        public int TrnNo { get; set; }
        public bool fInAct { get; set; }
        public byte fApr { get; set; }
        public short? SKy { get; set; }
        public int STrnKy { get; set; }
        public float RtnAmt { get; set; }
        public bool fFinish { get; set; }
        public int TrnTrfLnkKy { get; set; }
        public int RepAdrKy { get; set; }
        public int DriverKy { get; set; }
        public int FyKy { get; set; }
        public string? OurCd { get; set; }
        public string? DocNo { get; set; }
        public int OrdKy { get; set; }
        public int OrdDetKy { get; set; }
        public string? YurRef { get; set; }
        public DateTime? YurDt { get; set; }
        public int AdrKy { get; set; }
        public int AdrDetKy { get; set; }
        public int ContraAccKy { get; set; }
        public int AccKy { get; set; }
        public short PmtTrmKy { get; set; }
        public short PmtModeKy { get; set; }

        public decimal Amt { get; set; }
        public DateTime? DueDt { get; set; }
        public short CreditDys { get; set; }
        public short? BUKy { get; set; }
        public short LocKy { get; set; }
        public short? ShiftKy { get; set; }
        public decimal DisAmt { get; set; }
        public decimal? TtlDis { get; set; }
        public float DisPer { get; set; }
        public float ComisPer { get; set; }
        public bool fTax { get; set; }
        public bool fQtyPstd { get; set; }
        public bool fValPstd { get; set; }
        public bool fItmTrnVal { get; set; }
        public bool fPrint { get; set; }
        public bool TMf1 { get; set; }
        public bool TMf2 { get; set; }
        public byte tItmTrnRel { get; set; }
        public string? Des { get; set; }
        public decimal Amt1 { get; set; }
        public decimal Amt2 { get; set; }
        public decimal Amt3 { get; set; }
        public decimal Amt4 { get; set; }
        public float ChqAmt { get; set; }
        public float CreditAmt { get; set; }
        public string? OthNo { get; set; }
        public int CostCntrKy { get; set; }
        public int PrntKy { get; set; }
        public string? Status { get; set; }
        public short? AcsLvlKy { get; set; }
        public int? EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }
    }
}
