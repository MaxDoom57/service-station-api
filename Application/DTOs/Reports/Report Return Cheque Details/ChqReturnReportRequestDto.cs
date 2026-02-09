using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Return_Cheque_Details
{
    public class ChqReturnReportRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // cboAccCd.BoundText
        public int? AccKy { get; set; }
    }
}
