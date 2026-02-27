using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CustomerAccount
    {
        public short CKy { get; set; }
        public int CusAccKy { get; set; }
        public string? CusAccCd { get; set; }
        public string? CusAccNm { get; set; }
        //public bool fBlckList { get; set; }
    }
}
