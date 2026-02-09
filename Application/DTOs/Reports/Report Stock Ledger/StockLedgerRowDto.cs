using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Stock_Ledger
{
    public class StockLedgerRowDto
    {
        public DateOnly TrnDt { get; set; }
        public string? ItmCd { get; set; }
        public string? ItmNm { get; set; }
        public decimal Qty { get; set; }
        public decimal Balance { get; set; }
    }
}
