using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reports
{
    public class LSSvewInvRpt
    {
        public int TrnKy { get; set; }
        public int AdrKy { get; set; }
        public DateTime TrnDt { get; set; }
        public int TrnNo { get; set; }
        public short TrnTypKy { get; set; }
        public string? OurCd { get; set; }
        public string? DocNo { get; set; }
        public decimal? TtlDis { get; set; }
        public string? Des { get; set; }
        public int PrntKy { get; set; }
        public decimal DisAmt { get; set; }
        public short PmtTrmKy { get; set; }
        public int ContraAccKy { get; set; }
        public int RepAdrKy { get; set; }
        public string? TrnYurRef { get; set; }
        public int OrdKy { get; set; }
        public string? OrdDocNo { get; set; }
        public string? OrdYurRef { get; set; }
        public decimal Amt { get; set; }

        public int ItmKy { get; set; }
        public string ItmCd { get; set; } = string.Empty;
        public string? PartNo { get; set; }
        public double Qty { get; set; }
        public double Qty2 { get; set; }
        public short TrnUnitKy { get; set; }
        public decimal? TrnPri { get; set; }
        public int ItmTrnKy { get; set; }
        public short ItmUnitKy { get; set; }
        public decimal ItmCosPri { get; set; }
        public decimal? TrnCosPri { get; set; }
        public decimal ItmSlsPri { get; set; }
        public decimal? TrnSlsPri { get; set; }
        public string? ItmNm { get; set; }
        public short LiNo { get; set; }

        public DateTime? OrdDt { get; set; }
        public int? OrdNo { get; set; }
        public string? Rep { get; set; }

        public double IntQty { get; set; }
        public double FrctQty { get; set; }
        public string? TP1 { get; set; }
        public double? Total { get; set; }

        public string? CNm { get; set; }
        public string? Unit { get; set; }
        public string? AdrCd { get; set; }
        public string? CtPerson { get; set; }
        public string? AdrNm { get; set; }
        public string? FstNm { get; set; }
        public string? MidNm { get; set; }
        public string? LstNm { get; set; }
        public string? Address { get; set; }
        public string? Town { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        public short CKy { get; set; }

        public decimal Amt1 { get; set; }
        public decimal Amt2 { get; set; }
        public decimal Amt3 { get; set; }
        public decimal Amt4 { get; set; }

        public string? VType { get; set; }
        public string? CID1 { get; set; }
        public string PmtTerm { get; set; } = string.Empty;
        public string? AccCd { get; set; }
        public string PmtTrm { get; set; } = string.Empty;
        public string? ComAdr { get; set; }
        public string? Moto { get; set; }
        public float DisPer { get; set; }
        public short LocKy { get; set; }
        public string? CusItmCd { get; set; }
        public string? ComTp1 { get; set; }
        public string? ComTp2 { get; set; }
        public string? ItmCat2 { get; set; }
        public int AccKy { get; set; }
        public float SO { get; set; }
        public string? AdrID1 { get; set; }
        public int AdrDetKy { get; set; }
    }
}
