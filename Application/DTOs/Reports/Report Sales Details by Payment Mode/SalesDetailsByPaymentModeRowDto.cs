using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Details_by_Payment_Mode
{
    public class SalesDetailsByPaymentModeRowDto
    {
        public DateOnly TrnDt { get; set; }
        public int TrnNo { get; set; }

        public string? PmtModeCd { get; set; }
        public string? PmtModeNm { get; set; }

        public string? AccCd { get; set; }
        public string? AccNm { get; set; }

        public decimal Amt { get; set; }
        public string? UsrId { get; set; }
    }
}
