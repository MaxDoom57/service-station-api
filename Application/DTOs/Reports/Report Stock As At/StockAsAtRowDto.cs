using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Stock_As_At
{
    public class StockAsAtRowDto
    {
        public string ItmCd { get; set; } = "";
        public string? ItmNm { get; set; }
        public string? PartNo { get; set; }

        public double? Qty { get; set; }

        public string? Unit { get; set; }

        public decimal ItmCosPri { get; set; }
        public decimal ItmSlsPri { get; set; }

        public string? LocNm { get; set; }
        public string? BUNm { get; set; }

        public string? ItmCat1Nm { get; set; }
        public string? ItmCat2Nm { get; set; }
        public string? ItmCat3Nm { get; set; }
        public string? ItmCat4Nm { get; set; }
    }
}
