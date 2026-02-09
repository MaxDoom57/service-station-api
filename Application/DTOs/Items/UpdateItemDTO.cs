using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Items
{
    public class UpdateItemDTO
    {
        public int itemKey { get; set; }                     
        public string? itemCode { get; set; }
        public string? itemType { get; set; }
        public string? partNo { get; set; }
        public string? itemName { get; set; }
        public string? description { get; set; }

        public short? itmCat1Ky { get; set; }
        public short? itmCat2Ky { get; set; }
        public short? itmCat3Ky { get; set; }
        public short? itmCat4Ky { get; set; }

        public short? unitKey { get; set; }

        public double? discountPrecentage { get; set; }
        public decimal? costPrice { get; set; }
        public decimal? salesPrice { get; set; }
        public double? discountAmount { get; set; }
        public double? quantity { get; set; }
        public bool? fInAct { get; set; }
    }
}
