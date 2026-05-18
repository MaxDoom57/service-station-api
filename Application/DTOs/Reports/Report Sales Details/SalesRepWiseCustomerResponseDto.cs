using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Details
{
    /// <summary>
    /// SalesRepWiseCustomerResponseDto class.
    /// </summary>
    public class SalesRepWiseCustomerResponseDto
    {
        public string ReportTitle { get; set; } = "";

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public List<SalesRepWiseCustomerRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
