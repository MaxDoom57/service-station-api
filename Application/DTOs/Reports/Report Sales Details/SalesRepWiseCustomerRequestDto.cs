using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Details
{
    public class SalesRepWiseCustomerRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // cboRepNm.BoundText
        public int? RepAdrKy { get; set; }

        // cboItmCd.BoundText
        public int? ItmKy { get; set; }

        // ChkSlsRtn.Value
        public bool OnlySales { get; set; }

        // optSummary.Value
        public bool IsSummary { get; set; }
    }
}
