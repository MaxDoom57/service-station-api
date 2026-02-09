using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Account_Ledger
{
    public class AccLedgerReportRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // cboAccCd.BoundText (mandatory in most cases)
        public int? AccKy { get; set; }

        // cboAccTyp.BoundText
        public short? AccTypKy { get; set; }

        public bool MultiCurrency { get; set; }   // chkMaltiCrn
        public bool LandScope { get; set; }        // chkLndScp

        // grouping
        public string? OrderBy { get; set; }       // "Account Code" | "Account Name"
        public bool PageBreak { get; set; }        // ChkPgBrk
    }
}
