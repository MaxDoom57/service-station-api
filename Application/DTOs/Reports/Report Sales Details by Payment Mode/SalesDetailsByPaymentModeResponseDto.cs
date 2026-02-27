using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Details_by_Payment_Mode
{
    public class SalesDetailsByPaymentModeResponseDto
    {
        public List<SalesDetailsByPaymentModeRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
