using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Gross_Profit
{
    public class GrossProfitItemLocationRowDto
    {
        // Keys for filtering
        public int ItmKy { get; set; }
        public int LocKy { get; set; }
        public int BUKy { get; set; }
        public int RepAdrKy { get; set; }
        public int ItmTypKy { get; set; }
        public int? ItmCat1Ky { get; set; }
        public int? ItmCat2Ky { get; set; }
        public int? ItmCat3Ky { get; set; }
        public int? ItmCat4Ky { get; set; }
        public int? AdrKy { get; set; }

        // Display fields
        public string ItmCd { get; set; } = string.Empty;
        public string ItmNm { get; set; } = string.Empty;
        public string LocCd { get; set; } = string.Empty;
        public string BUCd { get; set; } = string.Empty;
        public string ItmTypCd { get; set; } = string.Empty;
        public string ItmCat1Cd { get; set; } = string.Empty;
        public string ItmCat2Cd { get; set; } = string.Empty;
        public string ItmCat3Cd { get; set; } = string.Empty;
        public string ItmCat4Cd { get; set; } = string.Empty;
        public string SlsRepAdrNm { get; set; } = string.Empty;
        public string AdrCd { get; set; } = string.Empty;

        public decimal SalesAmount { get; set; }
        public decimal CostAmount { get; set; }
        public decimal GrossProfit { get; set; }
    }
}
