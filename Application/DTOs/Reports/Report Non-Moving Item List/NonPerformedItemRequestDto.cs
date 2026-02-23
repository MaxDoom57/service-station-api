using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Non_Moving_Item_List
{
    public class NonPerformedItemRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // cboCat1.BoundText
        public int? ItmCat1Ky { get; set; }

        // cboCat2.BoundText
        public int? ItmCat2Ky { get; set; }
    }
}
