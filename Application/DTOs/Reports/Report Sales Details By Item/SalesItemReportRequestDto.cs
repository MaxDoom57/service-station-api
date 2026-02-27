using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Details_By_Item
{
    public class SalesItemReportRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public int? AccKy { get; set; }     // cboAccCd.BoundText
        public int? ItmKy { get; set; }     // cboItmCd.BoundText

        public bool SalesOnly { get; set; } // ChkSlsRtn
        public bool Summary { get; set; }   // optSummary
    }
}
