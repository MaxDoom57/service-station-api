using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Account_Transaction_Details
{
    public class AccTrnDetailsReportResponseDto
    {
        public ReportContextDto Context { get; set; } = new();

        public decimal OpeningBalance { get; set; }

        public List<AccTrnDetailsRowDto> Rows { get; set; } = new();
    }
}
