using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reports
{
    public class vewSlsDetByPmtTrm
    {
        public int TrnKy { get; set; }

        public DateTime TrnDt { get; set; }

        public int TrnNo { get; set; }

        public bool fInAct { get; set; }

        public string? OurCd { get; set; }

        public string? PmtModeCd { get; set; }

        public string? PmtModeNm { get; set; }

        public string? AccCd { get; set; }

        public string? AccNm { get; set; }

        public decimal Amt { get; set; }

        public short LiNo { get; set; }

        public int AccKy { get; set; }

        public int EntUsrKy { get; set; }

        public string? UsrId { get; set; }
    }
}
