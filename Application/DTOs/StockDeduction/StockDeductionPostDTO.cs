using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.StockDeduction
{
    public class StockDeductionItemDTO
    {
        public int itemKey { get; set; }
        public double quantity { get; set; }
        public decimal costPrice { get; set; }
        public decimal salePrice { get; set; }
    }

    public class StockDeductionPostDTO
    {
        public DateTime trnDate { get; set; }
        public short locKey { get; set; }
        public string? description { get; set; }
        public List<StockDeductionItemDTO> items { get; set; }
    }
}
