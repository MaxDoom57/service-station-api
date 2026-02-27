using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Customers
{
    public class UpdateCustomerAddressDto
    {
        public int AdrKy { get; set; }

        public string? AdrNm { get; set; }
        public string? FstNm { get; set; }
        public string? MidNm { get; set; }
        public string? LstNm { get; set; }
        public string? Address { get; set; }
        public string? TP1 { get; set; }
        public string? TP2 { get; set; }
        public string? EMail { get; set; }
        public string? GPSLoc { get; set; }
    }
}
