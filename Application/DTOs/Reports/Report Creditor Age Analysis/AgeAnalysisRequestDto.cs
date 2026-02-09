using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Creditor_Age_Analysis
{
    public class AgeAnalysisRequestDto
    {
        // Dr or Cr
        public string ReportMode { get; set; } = "Dr";

        public DateOnly AsAtDate { get; set; }

        public bool OverDueOnly { get; set; }

        public int Day1 { get; set; }
        public int Day2 { get; set; }
        public int Day3 { get; set; }
        public int Day4 { get; set; }

        public int? AccKy { get; set; }
        public int? SlsRepAdrKy { get; set; }
        public int? AdrTyp2Ky { get; set; }
        public string? BUCd { get; set; }

        // Group By text
        public string? GroupBy { get; set; }
    }
}
