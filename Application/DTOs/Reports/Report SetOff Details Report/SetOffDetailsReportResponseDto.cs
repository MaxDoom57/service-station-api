using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_SetOff_Details_Report
{
    public class SetOffDetailsReportResponseDto
    {
        public ReportContextDto Context { get; set; } = new();

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public string VarName { get; set; } = "All Accounts";

        public List<SetOffDetailsReportRowDto> Rows { get; set; } = new();
    }
}
