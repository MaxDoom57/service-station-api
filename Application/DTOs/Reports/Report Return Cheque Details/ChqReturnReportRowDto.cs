using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Return_Cheque_Details
{
    public class ChqReturnReportRowDto
    {
        public DateTime TrnDt { get; set; }
        public int TrnNo { get; set; }
        public string? OurCd { get; set; }
        public string? DocNo { get; set; }
        public string? Des { get; set; }
        public string? ChqNo { get; set; }
        public short LiNo { get; set; }
        public decimal Amt { get; set; }
        public int AccKy { get; set; }
    }
}
