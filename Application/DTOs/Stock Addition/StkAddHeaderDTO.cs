using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Stock_Addition
{
    public class StkAddHeaderDTO
    {
        public short LocKy { get; set; }
        public DateTime? TrnDt { get; set; }
        public string? Des { get; set; }
        public int TrnKy { get; set; }
    }
}
