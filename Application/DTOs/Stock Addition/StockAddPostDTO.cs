using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Stock_Addition
{
    public class StockAddDetailInput
    {
        public int itemKey { get; set; }
        public double quantity { get; set; }
        public decimal costPrice { get; set; }
        public decimal salePrice { get; set; }
        public int unitKey { get; set; }
    }

    public class StockAddPostDTO
    {
        public int? trnNo { get; set; }           // null = NEW, value = UPDATE
        public DateTime trnDate { get; set; }
        public short locKey { get; set; }
        public string? description { get; set; }
        public List<StockAddDetailInput> items { get; set; }
    }
}
