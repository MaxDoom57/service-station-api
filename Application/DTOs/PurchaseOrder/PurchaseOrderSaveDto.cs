using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PurchaseOrder
{
    public class PurchaseOrderSaveDto
    {
        public DateTime OrdDt { get; set; }
        public int AdrKy { get; set; }
        public int AccKy { get; set; }
        public string DocNo { get; set; } = string.Empty;
        public string? Des { get; set; }
        public int OrdTypKy { get; set; }
        public int OrdFrqKy { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new();
    }
}
