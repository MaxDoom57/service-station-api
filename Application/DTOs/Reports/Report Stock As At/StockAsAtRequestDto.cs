using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Stock_As_At
{
    public class StockAsAtRequestDto
    {
        public int LocKy { get; set; }          // cboLoc.BoundText (required)

        public int? BUKy { get; set; }           // cboBU
        public int? ItmTypKy { get; set; }       // cboItmTyp
        public int? ItmCat1Ky { get; set; }
        public int? ItmCat2Ky { get; set; }
        public int? ItmCat3Ky { get; set; }
        public int? ItmCat4Ky { get; set; }

        public bool IsCurrentStockQty { get; set; } = true; // chkCurStkQty
    }
}
