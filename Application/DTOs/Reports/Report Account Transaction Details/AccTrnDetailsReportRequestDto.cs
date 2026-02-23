using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Account_Transaction_Details
{
    public class AccTrnDetailsReportRequestDto
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }

        public int? FromTrnNo { get; set; }
        public int? ToTrnNo { get; set; }

        public string? FromDocNo { get; set; }
        public string? ToDocNo { get; set; }

        public string? FromYurRef { get; set; }
        public string? ToYurRef { get; set; }

        public int? AccKy { get; set; }
        public short? AccTypKy { get; set; }
        public short? TrnTypKy { get; set; }

        public bool IsContra { get; set; }
        public bool IsSummary { get; set; }
        public bool IsLandscape { get; set; }
    }
}
