using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Creditor_Age_Analysis
{
    public class AgeAnalysisResponseDto
    {
        public ReportContextDto Context { get; set; } = new();

        public string ReportTitle { get; set; } = string.Empty;

        public DateOnly AsAtDate { get; set; }

        public List<AgeAnalysisRowDto> Rows { get; set; } = new();
    }
}
