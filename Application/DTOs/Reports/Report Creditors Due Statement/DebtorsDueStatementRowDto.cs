using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Creditors_Due_Statement
{
    public class DebtorsDueStatementRowDto
    {
        public int AccKy { get; set; }
        public string? AccCd { get; set; }
        public string? AccNm { get; set; }

        public DateOnly? DueDt { get; set; }
        public decimal Amount { get; set; }

        public string? TrnTypCd { get; set; }
    }
}
