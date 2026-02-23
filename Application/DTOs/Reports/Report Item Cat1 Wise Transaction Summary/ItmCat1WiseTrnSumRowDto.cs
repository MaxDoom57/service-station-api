using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Item_Cat1_Wise_Transaction_Summary
{
    public class ItmCat1WiseTrnSumRowDto
    {
        public short ItmCat1Ky { get; set; }
        public string? ItmCat1Cd { get; set; }
        public string? ItmCat1Nm { get; set; }

        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }
}
