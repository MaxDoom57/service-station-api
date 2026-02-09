using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Company
    {
        [Key]
        public short CKy { get; set; }                    // smallint, not nullable

        [MaxLength(15)]
        public string? CCd { get; set; }                 // varchar(15)

        public bool fInAct { get; set; }                 // bit, not nullable

        public byte fApr { get; set; }                   // tinyint (maps to byte)

        [MaxLength(60)]
        public string? CNm { get; set; }                 // varchar(60)

        public byte[]? Logo { get; set; }                // image

        [MaxLength(60)]
        public string? Moto { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(60)]
        public string? Town { get; set; }

        [MaxLength(60)]
        public string? City { get; set; }

        [MaxLength(60)]
        public string? Country { get; set; }

        [MaxLength(30)]
        public string? TP1 { get; set; }

        [MaxLength(30)]
        public string? TP2 { get; set; }

        [MaxLength(30)]
        public string? TP3 { get; set; }

        [MaxLength(14)]
        public string? Fax { get; set; }

        [MaxLength(60)]
        public string? EMail { get; set; }

        [MaxLength(60)]
        public string? WebSite { get; set; }

        [MaxLength(30)]
        public string? CID1 { get; set; }

        [MaxLength(30)]
        public string? CID2 { get; set; }

        [MaxLength(30)]
        public string? CID3 { get; set; }

        [MaxLength(30)]
        public string? TaxNo { get; set; }

        public int PrntKy { get; set; }                  // int, not nullable

        public int? PrjObjKy { get; set; }               // int, nullable

        public DateTime? CurTrnDt { get; set; }          // smalldatetime

        public DateTime? TrnStDt { get; set; }           // smalldatetime

        public DateTime? FYStDt { get; set; }            // smalldatetime

        public DateTime? TrnCnfDt { get; set; }          // smalldatetime

        [MaxLength(2)]
        public string? Status { get; set; }              // char(2)

        public short SKy { get; set; }                   // smallint, not nullable

        public int? EntUsrKy { get; set; }               // int, nullable

        public DateTime? EntDtm { get; set; }            // smalldatetime

        public bool fPartNo { get; set; }                // bit, not nullable

        [Column(TypeName = "text")]
        public string? CRem { get; set; }                // text
    }
}
