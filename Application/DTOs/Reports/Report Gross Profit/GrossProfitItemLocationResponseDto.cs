using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Gross_Profit
{
    public class GrossProfitItemLocationResponseDto
    {
        public ReportContextDto Context { get; set; } = new();
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string GroupBy1 { get; set; } = string.Empty;
        public string GroupBy2 { get; set; } = string.Empty;
        public List<GrossProfitItemLocationRowDto> Rows { get; set; } = new();
    }
}
