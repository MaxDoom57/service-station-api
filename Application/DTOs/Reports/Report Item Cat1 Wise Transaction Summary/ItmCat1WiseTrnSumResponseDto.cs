using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Item_Cat1_Wise_Transaction_Summary
{
    public class ItmCat1WiseTrnSumResponseDto
    {
        public ReportContextDto Context { get; set; } = new();

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public short LocKy { get; set; }
        public short ItmTypKy { get; set; }

        public List<ItmCat1WiseTrnSumRowDto> Rows { get; set; } = new();
    }
}
