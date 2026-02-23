using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Item_Cat1_Wise_Transaction_Summary
{
    public class ItmCat1WiseTrnSumRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // cboLocNm.BoundText (required)
        public short LocKy { get; set; }

        // cboItmTyp.BoundText (required)
        public short ItmTypKy { get; set; }
    }
}
