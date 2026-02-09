using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Details_By_Item
{
    public class SalesItemRowDto
    {
        public DateOnly TrnDt { get; set; }

        public int AccKy { get; set; }
        public string? AccNm { get; set; }

        public int ItmKy { get; set; }
        public string? ItmNm { get; set; }

        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }
}
