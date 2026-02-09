using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Account_Ledger
{
    public class AccLedgerReportResponseDto
    {
        public ReportContextDto Context { get; set; } = new();

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public decimal OpeningBalance { get; set; }
        public decimal? OpeningForeignBalance { get; set; }

        public List<AccLedgerReportRowDto> Rows { get; set; } = new();
    }
}
