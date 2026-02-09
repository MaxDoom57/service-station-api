using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Credit_Sales_Summary
{
    public class LssCreditInvoiceSummaryRowDto
    {
        public DateTime TrnDt { get; set; }
        public int TrnNo { get; set; }
        public string? DocNo { get; set; }

        public int AdrKy { get; set; }
        public string? AdrCd { get; set; }
        public string? AdrNm { get; set; }

        public decimal Amt { get; set; }
    }
}
