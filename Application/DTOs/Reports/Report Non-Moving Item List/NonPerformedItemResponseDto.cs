using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Non_Moving_Item_List
{
    public class NonPerformedItemResponseDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public List<NonPerformedItemRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
