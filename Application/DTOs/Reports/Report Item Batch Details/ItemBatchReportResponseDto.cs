using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Item_Batch_Details
{
    public class ItemBatchReportResponseDto
    {
        public string ReportTitle { get; set; } = "ITEM BATCH DETAILS REPORT";

        public List<ItemBatchReportRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
