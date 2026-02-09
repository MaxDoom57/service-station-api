using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Customer_Address_Label
{
    public class AccountAddressDto
    {
        // Account
        public int AccKy { get; set; }
        public string? AccCd { get; set; }
        public string? AccNm { get; set; }
        public string? AccTyp { get; set; }

        // Address
        public int AdrKy { get; set; }
        public string? AdrCd { get; set; }
        public string? AdrNm { get; set; }
        public string AdrTyp { get; set; } = null!;
        public string? ActNm { get; set; }

        // Contact
        public string? TP1 { get; set; }
        public string? TP2 { get; set; }
        public string? EMail { get; set; }
        public string? City { get; set; }
    }
}
