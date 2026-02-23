using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Sales_Summary_By_Rep
{
    public class SalesByRepTransactionDto
    {
        public DateTime TrnDt { get; set; }
        public int TrnNo { get; set; }
        public string? SlsRepAdrNm { get; set; }

        public List<SalesByRepItemDto> Items { get; set; } = new();
    }
}
