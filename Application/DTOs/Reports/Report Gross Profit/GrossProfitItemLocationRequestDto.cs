using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Gross_Profit
{
    public class GrossProfitItemLocationRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public int? ItmKy { get; set; }
        public int? LocKy { get; set; }
        public int? BUKy { get; set; }
        public int? RepAdrKy { get; set; }
        public int? ItmTypKy { get; set; }
        public int? ItmCat1Ky { get; set; }
        public int? ItmCat2Ky { get; set; }
        public int? ItmCat3Ky { get; set; }
        public int? ItmCat4Ky { get; set; }
        public int? AdrKy { get; set; }

        public string? GroupBy1 { get; set; }
        public string? GroupBy2 { get; set; }
    }
}
