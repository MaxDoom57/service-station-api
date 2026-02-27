using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Details_by_Payment_Mode
{
    public class SalesDetailsByPaymentModeRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // cboPmtMod.Text
        public string? PaymentModeName { get; set; }
    }
}
