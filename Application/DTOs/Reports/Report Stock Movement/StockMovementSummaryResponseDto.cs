using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Stock_Movement
{
    public class StockMovementSummaryResponseDto
    {
        public string ReportTitle { get; set; } = "Stock Movement Summary";

        public List<StockMovementSummaryRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
