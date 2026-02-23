using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Creditors_Due_Statement
{
    public class DebtorsDueStatementRequestDto
    {
        public DateOnly AsAtDate { get; set; }

        // cboAccCd.BoundText
        public int? AccKy { get; set; }

        // chkOverdue
        public bool IsOverdue { get; set; }

        // cboTrnTyp.Text
        public string? TrnTypCd { get; set; }

        // Form Tag (for vewObjPropDet)
        public string FormTag { get; set; } = string.Empty;
    }
}
