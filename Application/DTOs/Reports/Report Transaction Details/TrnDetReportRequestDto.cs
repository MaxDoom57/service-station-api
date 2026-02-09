using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Summary_By_Rep
{
    public class TrnDetReportRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public string? TrnTyp { get; set; }
        public string? AdrNm { get; set; }
        public string? ItmNm { get; set; }

        public bool FreeItemsOnly { get; set; }

        public int? RepAdrKy { get; set; }

        // UI flags
        public bool ShowDetails { get; set; }   // chkDet
        public bool Summary { get; set; }       // chkSum
        public string? OrderBy { get; set; }    // cboOrdBy
    }
}
