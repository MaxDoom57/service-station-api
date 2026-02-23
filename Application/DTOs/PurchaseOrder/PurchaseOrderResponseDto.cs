using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PurchaseOrder
{
    public class PurchaseOrderResponseDto
    {
        public PurchaseOrderHeaderDto Header { get; set; } = null!;
        public List<PurchaseOrderDetailDto> Details { get; set; } = new();
        public decimal TaxAmount { get; set; }
    }
}
