using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class AdrMas
    {
        public int AdrKy { get; set; }
        public short CKy { get; set; }
        public string? AdrCd { get; set; }
        public bool? fInAct { get; set; }
        public byte? fApr { get; set; }
        public bool? fCusSup { get; set; }
        [NotMapped]
        public bool fDefault { get; set; }
        public string? CtPerson { get; set; }
        public string AdrNm { get; set; }
        public string? FstNm { get; set; }
        public string? MidNm { get; set; }
        public string? LstNm { get; set; }
        public string? Initial { get; set; }
        public short? TitleKy { get; set; }
        public string? TitleQly { get; set; }
        public string? Desg { get; set; }
        public string? Address { get; set; }
        public string? Town { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        public string? ZipCd { get; set; }
        public string? TaxNo { get; set; }
        public string? AdrID1 { get; set; }
        public int? SlsRepKy { get; set; }
        public short? AdrCat1Ky { get; set; }
        public short? AdrCat2Ky { get; set; }
        public short? AdrCat3Ky { get; set; }
        public short? AdrCat4Ky { get; set; }
        public short? AdrCtCatKy { get; set; }
        public short? AdrPriCatKy { get; set; }
        public string? TP1 { get; set; }
        public string? TP2 { get; set; }
        public string? TP3 { get; set; }
        public float? AdrNo1 { get; set; }
        public string? Fax { get; set; }
        public string? EMail { get; set; }
        public string? WebSite { get; set; }
        public string? AdrRem { get; set; }
        public string? Status { get; set; }
        public short? SKy { get; set; }
        public short? AdrDesgky { get; set; }
        public int? EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }
        public DateTime? NxtDt { get; set; }
        public bool? fPrint { get; set; }
        [NotMapped]
        public string? ActNm { get; set; }
    }
}
