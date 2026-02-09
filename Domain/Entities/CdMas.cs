using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class CdMas
    {
        [Key]
        public short CdKy { get; set; }

        public short CKy { get; set; }
        public short ConKy { get; set; }
        public string Code { get; set; }
        public bool fInAct { get; set; }
        public byte fApr { get; set; }
        public string ConCd { get; set; }
        public string CdNm { get; set; }
        public bool fCtrlCd { get; set; }
        public short CtrlCdKy { get; set; }
        public string? OurCd { get; set; }
        public int ObjKy { get; set; }
        public short AcsLvlKy { get; set; }

        public float SO { get; set; }

        public bool fUsrAcs { get; set; }
        public bool fCCAcs { get; set; }
        public bool fDefault { get; set; }
        public bool CdF1 { get; set; }
        public bool CdF2 { get; set; }
        public bool CdF3 { get; set; }
        public bool CdF4 { get; set; }
        public bool CdF5 { get; set; }
        public bool CdF6 { get; set; }
        public bool CdF7 { get; set; }
        public bool CdF8 { get; set; }
        public bool CdF9 { get; set; }
        public bool CdF10 { get; set; }
        public bool CdF11 { get; set; }
        public bool CdF12 { get; set; }
        public bool CdF13 { get; set; }
        public bool CdF14 { get; set; }
        public bool CdF15 { get; set; }

        public int CdInt1 { get; set; }
        public int CdInt2 { get; set; }
        public int CdInt3 { get; set; }

        public float CdNo1 { get; set; }
        public float CdNo2 { get; set; }
        public float CdNo3 { get; set; }
        public float CdNo4 { get; set; }
        public float CdNo5 { get; set; }

        public DateTime? CdDt1 { get; set; }
        public DateTime? CdDt2 { get; set; }
        public DateTime? CdDt3 { get; set; }

        public int? PrntKy { get; set; }
        public string? Status { get; set; }
        public short SKy { get; set; }

        public int EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }

        public int CdInt4 { get; set; }
    }
}
