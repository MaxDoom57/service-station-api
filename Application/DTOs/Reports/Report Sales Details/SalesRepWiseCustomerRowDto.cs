using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Details
{
    public class SalesRepWiseCustomerRowDto
    {
        public DateOnly TrnDt { get; set; }

        public int TrnNo { get; set; }

        public string? AccNm { get; set; }

        public string? ItmNm { get; set; }

        public decimal Qty { get; set; }

        public decimal Amt { get; set; }
    }
}
