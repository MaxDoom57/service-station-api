using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Core.Entities.Reports
{
    [Table("SetOffViewQry")]
    public class SetOffViewQry
    {
        public DateTime? SetOffDt { get; set; }

        public int? SetOffNo { get; set; }

        public int? LnNo { get; set; }

        public DateTime? DrTrnDate { get; set; }

        public int? DrTrnNo { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? DrTrnTyp { get; set; }

        [Column(TypeName = "money")]
        public decimal? DrAmt { get; set; }

        public DateTime CrTrnDt { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string CrTrnTyp { get; set; } = string.Empty;

        public int CrTrnNo { get; set; }

        [Column(TypeName = "money")]
        public decimal? CrAmt { get; set; }

        [Column(TypeName = "money")]
        public decimal? SetOffAmt { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? AdrNm { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? AdrCd { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? ChqNo { get; set; }

        public DateTime? ChqDt { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? BnkNm { get; set; }

        [Column(TypeName = "money")]
        public decimal? ChqAmt { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? BrnNm { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? CNm { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? Address { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? DrDocNo { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? CrDocNo { get; set; }

        public int AccKy { get; set; }

        public int CrTrnKy { get; set; }

        public short CrLiNo { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? TP1 { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? TP2 { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? EMail { get; set; }

        [Column(TypeName = "varchar(14)")]
        public string? Fax { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? DrAccCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? DrAccNm { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? CrAccCd { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? CrAccNm { get; set; }

        [Column(TypeName = "varchar(120)")]
        public string? CrAccTrnDes { get; set; }

        [Column(TypeName = "varchar(120)")]
        public string? CustAdrNo { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? CustAdrTown { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? CustAdrCity { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? CustCountry { get; set; }
    }
}
