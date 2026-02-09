using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Non_Active_Customer_List
{
    public class NonPerformedCustomerRowDto
    {
        public int AdrKy { get; set; }
        public string? AdrCd { get; set; }
        public string? AdrNm { get; set; }
        public string? AccCd { get; set; }
        public string? AccNm { get; set; }
    }
}
