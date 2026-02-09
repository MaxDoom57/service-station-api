using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Stock_As_At
{
    public class StockAsAtResponseDto
    {
        public string ReportTitle { get; set; } = "STOCK AS AT";

        public List<StockAsAtRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
