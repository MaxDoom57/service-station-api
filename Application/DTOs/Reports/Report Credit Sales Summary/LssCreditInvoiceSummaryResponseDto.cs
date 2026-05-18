using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Credit_Sales_Summary
{
    /// <summary>
    /// LssCreditInvoiceSummaryResponseDto class.
    /// </summary>
    public class LssCreditInvoiceSummaryResponseDto
    {
        public ReportContextDto Context { get; set; } = new();

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public List<LssCreditInvoiceSummaryRowDto> Rows { get; set; } = new();
    }
}
