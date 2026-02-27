using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reports
{
    public class vewAdrDet
    {
        public bool? fInAct { get; set; }
        public string? Status { get; set; }

        public int AdrKy { get; set; }
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

        public short? AdrCat2Ky { get; set; }
        public short? AdrCat3Ky { get; set; }
        public short? AdrCat4Ky { get; set; }

        public string? TP1 { get; set; }
        public string? TP2 { get; set; }
        public string? TP3 { get; set; }

        public string? Fax { get; set; }
        public string? EMail { get; set; }
        public string? WebSite { get; set; }

        public int? AccKy { get; set; }
        public int EntUsrKy { get; set; }

        public DateTime? EntDtm { get; set; }
        public byte? fApr { get; set; }

        public string? AdrCat2Cd { get; set; }
        public string? AdrCat3Nm { get; set; }
        public string? AdrCat4Nm { get; set; }

        public string? AdrID1 { get; set; }

        public string? AccCd { get; set; }
        public string? AccTyp { get; set; }
        public string? AccNm { get; set; }

        public string? AdrCat3Cd { get; set; }
        public string? AdrCat4Cd { get; set; }

        public float? DisAmt { get; set; }

        public short CKy { get; set; }

        public string? TaxNo { get; set; }
        public string? Initial { get; set; }

        public string AdrTyp { get; set; } = "";
        public short AdrTypKy { get; set; }

        public short? TitleKy { get; set; }

        public string? AdrPriCatCd { get; set; }
        public short? AdrPriCatKy { get; set; }

        public bool? fCusSup { get; set; }

        public string? SlsRepAdrCd { get; set; }
        public string? SlsRepAdrNm { get; set; }
        public int? SlsRepKy { get; set; }

        public string? Title { get; set; }

        public string? AdrRem { get; set; }

        public DateTime? NxtDt { get; set; }
        public bool? fPrint { get; set; }

        public string? ActNm { get; set; }
        public string? Desg { get; set; }

        public string? Adr1 { get; set; }
        public string? Adr2 { get; set; }
        public string? Adr3 { get; set; }

        public string? District { get; set; }
        public string? PoliceDiv { get; set; }
        public string? GSArea { get; set; }
        public string? NrstTown { get; set; }

        public int BirdCpacty { get; set; }
        public int FlrAra { get; set; }

        public string? Sex { get; set; }
        public DateTime? DateOfB { get; set; }

        public string? ZipCd { get; set; }

        public float? AdrNo1 { get; set; }

        public bool fDefault { get; set; }

        public string? GPSLoc { get; set; }
    }
}
