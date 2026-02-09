using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Stock_Ledger
{
    public class StockLedgerResponseDto
    {
        public string ReportTitle { get; set; } = "STOCK LEDGER";

        public decimal OpeningBalance { get; set; }

        public List<StockLedgerRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
