using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reports
{
    public class vewAccAdrDet
    {
        public int AccKy { get; set; }
        public int AdrKy { get; set; }

        public string? AccCd { get; set; }
        public string? AccNm { get; set; }
        public string? AccTyp { get; set; }

        public string? AdrCd { get; set; }
        public string? AdrNm { get; set; }

        public string AdrTyp { get; set; } = null!;

        public short CKy { get; set; }

        public string? ActNm { get; set; }

        public short AdrTypKy { get; set; }
    }
}
