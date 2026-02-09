using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Item_Batch_Details
{
    public class ItemBatchReportRowDto
    {
        public int ItmKy { get; set; }
        public string ItmCd { get; set; } = null!;
        public string? ItmNm { get; set; }

        public string? PartNo { get; set; }

        public int ItmBatchKy { get; set; }
        public string? BatchNo { get; set; }

        public DateOnly? ExpirDt { get; set; }

        public float CosPri { get; set; }
        public float SalePri { get; set; }

        public float Qty { get; set; }
        public double? ItmLocQty { get; set; }
    }
}
