using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ItemBatch
{
    public class UpdateItemBatchDTO
    {
        public required int itemBatchKey { get; set; } 
        public required int itemKey { get; set; }    

        public string? batchNo { get; set; }
        public DateTime? expirDt { get; set; }
        public float? costPrice { get; set; }
        public float? salePrice { get; set; }
        public float? quantity { get; set; }
    }
}
