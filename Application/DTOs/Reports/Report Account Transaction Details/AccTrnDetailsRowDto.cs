using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Account_Transaction_Details
{
    public class AccTrnDetailsRowDto
    {
        public DateTime TrnDt { get; set; }
        public int TrnNo { get; set; }
        public string TrnType { get; set; } = string.Empty;
        public string? DocNo { get; set; }
        public string? YurRef { get; set; }
        public string? Des { get; set; }
        public string? ChqNo { get; set; }

        public decimal Amt { get; set; }
        public decimal? FrnAmt { get; set; }

        public string? AccCd { get; set; }
        public string? AccNm { get; set; }
    }
}
