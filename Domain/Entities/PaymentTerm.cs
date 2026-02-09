using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PaymentTerm
    {
        public short PmtTrmKy { get; set; } 
        public string PmtTrmCd { get; set; }
        public string PmtTrmNm { get; set; }
        public short CKy { get; set; }
        public string? PmtTrm { get; set; }
    }
}
