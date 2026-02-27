using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Summary_By_Rep
{
    public class ReportSalesByRepResponseDto
    {
        public ReportContextDto Context { get; set; } = new();

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? RepName { get; set; }
        public int? ItemKy { get; set; }
        public bool SalesReturn { get; set; }
        public string ViewType { get; set; } = "details";

        public List<SalesByRepTransactionDto> Transactions { get; set; } = new();
    }
}
