using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Credit_Sales_Summary
{
    public class LssCreditInvoiceSummaryRequestDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Optional: cboAdrCd
        public int? AdrKy { get; set; }
    }
}
