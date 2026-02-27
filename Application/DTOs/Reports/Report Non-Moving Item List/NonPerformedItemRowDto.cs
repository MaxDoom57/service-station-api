using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Non_Moving_Item_List
{
    public class NonPerformedItemRowDto
    {
        public int ItmKy { get; set; }
        public string ItmCd { get; set; } = "";
        public string? ItmNm { get; set; }

        public int ItmCat1Ky { get; set; }
        public int ItmCat2Ky { get; set; }
    }
}
