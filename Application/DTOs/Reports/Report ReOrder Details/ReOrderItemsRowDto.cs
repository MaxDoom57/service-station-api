using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_ReOrder_Details
{
    public class ReOrderItemsRowDto
    {
        public int ItmKy { get; set; }
        public string ItmCd { get; set; } = "";
        public string? PartNo { get; set; }
        public string? ItmNm { get; set; }

        public float ReOrdLvl { get; set; }
        public float ItmLocQty { get; set; }
        public float ReOrdQty { get; set; }

        public string ItmTyp { get; set; } = "";
        public DateOnly TrnDt { get; set; }
        public int TrnNo { get; set; }
    }
}
