using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Transaction_Details
{
    public class TrnDetReportRowDto
    {
        public DateTime TrnDt { get; set; }
        public int TrnNo { get; set; }
        public string TrnTyp { get; set; } = string.Empty;
        public string CdNm { get; set; } = string.Empty;
        public string? DocNo { get; set; }
        public string? YurRef { get; set; }
        public string? AdrNm { get; set; }

        public string ItmCd { get; set; } = string.Empty;
        public string? ItmNm { get; set; }
        public double Qty { get; set; }
        public string? Unit { get; set; }

        public decimal TrnPri { get; set; }
        public decimal Amt1 { get; set; }
        public decimal Amt2 { get; set; }
        public decimal DisAmt { get; set; }
        public decimal Amt3 { get; set; }

        public string? ItmCat1Nm { get; set; }
        public string? BUNm { get; set; }
        public string? UsrId { get; set; }
    }
}
