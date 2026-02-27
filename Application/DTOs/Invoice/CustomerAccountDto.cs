using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Invoice
{
    public class CustomerAccountDto
    {
        public int CusAccKy { get; set; }
        public string? CusAccCd { get; set; }
        public string? CusAccNm { get; set; }
    }
}
