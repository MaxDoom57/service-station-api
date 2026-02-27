using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrdDet
    {
        [Key]
        public int OrdDetKy { get; set; }

        public int Ordky { get; set; }

        public int AdrKy { get; set; }

        public double LiNo { get; set; }

        public int? ItmKy { get; set; }

        [StringLength(15)]
        public string? ItmCd { get; set; }

        [StringLength(120)]
        public string? Des { get; set; }

        [StringLength(2)]
        public string? Status { get; set; }

        public double? EstQty { get; set; }

        public double? OrdQty { get; set; }

        public double? DlvQty { get; set; }

        public double BulkQty { get; set; }

        [Column(TypeName = "money")]
        public decimal EstPri { get; set; }

        [Column(TypeName = "money")]
        public decimal OrdPri { get; set; }

        [Column(TypeName = "money")]
        public decimal? CosPri { get; set; }

        [Column(TypeName = "money")]
        public decimal? SlsPri { get; set; }

        public float DisPer { get; set; }

        [Column(TypeName = "money")]
        public decimal DisAmt { get; set; }

        public byte? fApr { get; set; }

        public bool fVirtItm { get; set; }

        [StringLength(15)]
        public string? OrdItmCd { get; set; }

        public short? OrdStsKy { get; set; }

        public short OrdUnitKy { get; set; }

        public double BulkFctr { get; set; }

        public DateTime? ReqDt { get; set; }

        public short? BulkUnitKy { get; set; }

        public int StkStsKy { get; set; }

        [Column(TypeName = "numeric(18,0)")]
        public decimal? GrsWt { get; set; }

        [Column(TypeName = "numeric(18,0)")]
        public decimal NetWt { get; set; }

        public short BUKy { get; set; }

        public short CdKy1 { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt1 { get; set; }

        [Column(TypeName = "money")]
        public decimal Amt2 { get; set; }

        public bool fNoPrnPri { get; set; }

        public int? EntUsrKy { get; set; }

        public DateTime? EntDtm { get; set; }

        public byte? FmtFntSize { get; set; }

        public bool? FmtFntUndLn { get; set; }

        public byte? FmtFntStyle { get; set; }

        public short? Cd2Ky { get; set; }

        public short? Cd3Ky { get; set; }

        public bool? FmtPrtPri { get; set; }

        [Column(TypeName = "text")]
        public string? Rem { get; set; }

        [StringLength(15)]
        public string? SpecCd { get; set; }

        public bool isMatSub { get; set; }

        public bool isLabSub { get; set; }

        public bool isPltSub { get; set; }

        [Column(TypeName = "money")]
        public decimal MatAmt { get; set; }

        [Column(TypeName = "money")]
        public decimal LabAmt { get; set; }

        [Column(TypeName = "money")]
        public decimal PltAmt { get; set; }

        [Column(TypeName = "money")]
        public decimal SubConAmt { get; set; }

        public short SubOHP { get; set; }
    }
}
