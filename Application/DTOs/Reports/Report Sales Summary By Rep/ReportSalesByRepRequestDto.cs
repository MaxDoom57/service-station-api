using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Summary_By_Rep
{
    public class ReportSalesByRepRequestDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string? RepName { get; set; }
        public int? ItemKy { get; set; }

        public bool SalesReturn { get; set; }

        // "summary" or "details"
        public string ViewType { get; set; } = "details";
    }
}
