using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Stock_Ledger
{
    public class StockLedgerRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // cboItmCd.BoundText
        public int ItmKy { get; set; }

        // cboItmTypCd.BoundText
        public int? ItmTypKy { get; set; }

        // cboLoc.BoundText (required)
        public int LocKy { get; set; }

        // cboOrdBy.Text
        public string? OrderBy { get; set; }

        // ChkPgBrk
        public bool PageBreak { get; set; }
    }
}
