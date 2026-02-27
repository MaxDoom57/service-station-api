using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Summary_By_Rep
{
    public class SalesByRepItemDto
    {
        public string? ItmCd { get; set; }
        public string? ItmNm { get; set; }
        public double Qty { get; set; }
        public decimal TrnPri { get; set; }
        public decimal DisAmt { get; set; }
    }
}
