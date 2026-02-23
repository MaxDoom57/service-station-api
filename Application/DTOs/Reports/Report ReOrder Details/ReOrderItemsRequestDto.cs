using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_ReOrder_Details
{
    public class ReOrderItemsRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public int? ItmKy { get; set; }
        public int? ItmCat1Ky { get; set; }
        public int? ItmCat2Ky { get; set; }

        // When these are provided, VB switches to vewReOrdDetBySup
        public int? ItmCat3Ky { get; set; }
        public int? AdrKy { get; set; }
    }
}
