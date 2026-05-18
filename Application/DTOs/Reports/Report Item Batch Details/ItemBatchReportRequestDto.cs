using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Item_Batch_Details
{
    /// <summary>
    /// ItemBatchReportRequestDto class.
    /// </summary>
    public class ItemBatchReportRequestDto
    {
        // cboItmCd.BoundText
        public int? ItmKy { get; set; }

        // dtpFrmDt
        public DateOnly? ExpiryDate { get; set; }
    }
}
