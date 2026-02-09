using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Customers
{
    public class CustomerDto
    {
        public string? AccTyp { get; set; }
        public string? AccNm { get; set; }
        public int AccKy { get; set; }
        public string? AdrNm { get; set; }
        public string? AdrCd { get; set; }
        public int AdrKy { get; set; }
        public string? NIC { get; set; }
        public string? Address { get; set; }
        public string? Town { get; set; }
        public string? City { get; set; }
        public string? TP1 { get; set; }
        public string? GPSLoc { get; set; }
    }
}
