using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_ReOrder_Details
{
    public class ReOrderItemsResponseDto
    {
        public string ReportTitle { get; set; } = "Re-Order Items Report";

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public List<ReOrderItemsRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
