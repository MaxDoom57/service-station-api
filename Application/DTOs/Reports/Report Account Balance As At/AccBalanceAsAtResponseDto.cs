using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Account_Balance_As_At
{
    public class AccBalanceAsAtResponseDto
    {
        public ReportContextDto Context { get; set; } = new();

        public DateOnly AsAtDate { get; set; }

        public List<AccBalanceAsAtRowDto> Rows { get; set; } = new();
    }
}
