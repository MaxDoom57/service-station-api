using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Invoice
{
    /// <summary>
    /// SalesAccountDto class.
    /// </summary>
    public class SalesAccountDto
    {
        public int AccKy { get; set; }
        public string? AccCd { get; set; }
        public string? AccNm { get; set; }
    }
}
