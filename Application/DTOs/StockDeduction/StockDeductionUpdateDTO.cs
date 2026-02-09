using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.StockDeduction
{
    public class StockDeductionUpdateDTO
    {
        public int trnNo { get; set; }
        public DateTime trnDate { get; set; }
        public short locKey { get; set; }
        public string? description { get; set; }
        public List<StockDeductionItemDTO> items { get; set; }
    }
}
