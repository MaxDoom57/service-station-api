using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Creditors_Due_Statement
{
    public class DebtorsDueStatementResponseDto
    {
        public string ReportTitle { get; set; } = string.Empty;

        public DateOnly AsAtDate { get; set; }

        public string ObjRemark { get; set; } = string.Empty;

        public List<DebtorsDueStatementRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
