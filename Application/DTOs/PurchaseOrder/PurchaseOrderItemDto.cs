using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PurchaseOrder
{
    public class PurchaseOrderItemDto
    {
        public int OrdDetKy { get; set; }      // 0 = new
        public int ItmKy { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public DateTime? ReqDt { get; set; }
        public string? Des { get; set; }
        public decimal TaxAmt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
