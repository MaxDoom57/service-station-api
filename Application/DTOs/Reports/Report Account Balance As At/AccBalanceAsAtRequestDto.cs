using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Account_Balance_As_At
{
    public class AccBalanceAsAtRequestDto
    {
        public DateOnly AsAtDate { get; set; }

        // cboAccNm.BoundText
        public int? AccKy { get; set; }

        // lstItmTyp multi select values
        public List<string>? AccTypes { get; set; }
    }
}
