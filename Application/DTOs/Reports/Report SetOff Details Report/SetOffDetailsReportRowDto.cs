using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_SetOff_Details_Report
{
    public class SetOffDetailsReportRowDto
    {
        public DateTime? SetOffDt { get; set; }
        public int? SetOffNo { get; set; }
        public int? LnNo { get; set; }

        public DateTime? DrTrnDate { get; set; }
        public int? DrTrnNo { get; set; }
        public string? DrTrnTyp { get; set; }
        public decimal? DrAmt { get; set; }

        public DateTime CrTrnDt { get; set; }
        public int CrTrnNo { get; set; }
        public string CrTrnTyp { get; set; } = string.Empty;
        public decimal? CrAmt { get; set; }

        public decimal? SetOffAmt { get; set; }

        public string? AdrNm { get; set; }
        public string? AdrCd { get; set; }
        public string? ChqNo { get; set; }
        public DateTime? ChqDt { get; set; }
        public string? BnkNm { get; set; }
        public decimal? ChqAmt { get; set; }

        public string? DrAccCd { get; set; }
        public string? DrAccNm { get; set; }
        public string? CrAccCd { get; set; }
        public string? CrAccNm { get; set; }
    }
}
