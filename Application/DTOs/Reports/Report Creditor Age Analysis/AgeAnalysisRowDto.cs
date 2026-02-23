using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Creditor_Age_Analysis
{
    public class AgeAnalysisRowDto
    {
        public int AccKy { get; set; }
        public string? AccCd { get; set; }
        public string? AccNm { get; set; }

        public string? AreaNm { get; set; }
        public string? SlsRepAdrNm { get; set; }

        public DateTime DueDt { get; set; }
        public decimal Balance { get; set; }
    }
}
