using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Invoice
{
    public class PaymentTermDto
    {
        public short PmtTrmKy { get; set; }
        public string PmtTrmCd { get; set; }
        public string PmtTrmNm { get; set; }
        public string? PmtTrm { get; set; }
    }
}
