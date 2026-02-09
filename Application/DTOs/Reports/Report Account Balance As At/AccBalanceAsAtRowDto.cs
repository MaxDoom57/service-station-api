using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Account_Balance_As_At
{
    public class AccBalanceAsAtRowDto
    {
        public int AccKy { get; set; }
        public string? AccCd { get; set; }
        public string? AccNm { get; set; }
        public string? AccTyp { get; set; }

        public decimal Balance { get; set; }
    }
}
