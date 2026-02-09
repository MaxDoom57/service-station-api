using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PurchaseOrder
{
    public class PurchaseOrderDetailDto
    {
        public int ItmKy { get; set; }
        public string ItmCd { get; set; } = string.Empty;
        public string ItmNm { get; set; } = string.Empty;
        public decimal OrdQty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal CosPri { get; set; }
        public decimal Amount { get; set; }
        public DateTime? ReqDt { get; set; }
        public string? Des { get; set; }
        public int OrdDetKy { get; set; }
        public decimal Amt1 { get; set; }
    }
}
