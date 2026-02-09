using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Transaction_Details
{
    public class TrnDetReportResponseDto
    {
        public ReportContextDto Context { get; set; } = new();

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string TrnTypLabel { get; set; } = "All";

        public List<TrnDetReportRowDto> Rows { get; set; } = new();
    }
}
