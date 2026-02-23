using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_SetOff_Details_Report
{
    public class SetOffDetailsReportRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // Customer / Supplier account (optional)
        public int? AccKy { get; set; }

        // Display name for report header
        public string? VarName { get; set; }
    }
}
