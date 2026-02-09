using System.ComponentModel.DataAnnotations;

public class Account
{
    [Key]
    public int AccKy { get; set; }
    public short CKy { get; set; }
    public string AccCd { get; set; }
    public bool fInAct { get; set; }
    public byte fApr { get; set; }
    public string AccNm { get; set; }
    public string AccTyp { get; set; }
    public short AccTypKy { get; set; }
    public byte AccLvl { get; set; }
    public short CrnKy { get; set; }
    public byte fCusSup { get; set; }
    public bool fCtrlAcc { get; set; }
    public int CtrlAccKy { get; set; }
    public short AcsLvlKy { get; set; }
    public bool fBasAcc { get; set; }
    public bool fMultiAdr { get; set; }
    public bool fDefault { get; set; }
    public bool fBlckList { get; set; }
    public decimal CrLmt { get; set; }
    public short CrDays { get; set; }
    public short AccCat1Ky { get; set; }
    public short AccCat2Ky { get; set; }
    public short AccCat3Ky { get; set; }
    public int BrnKy { get; set; }
    public string BnkAccNo { get; set; }
    public string BnkAccNm { get; set; }
    public short BUKy { get; set; }
    public short SKy { get; set; }
    public string Status { get; set; }
    public int EntUsrKy { get; set; }
    public DateTime? EntDtm { get; set; }
}
