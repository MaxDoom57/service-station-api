using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PurchaseOrder
{
    public class PurchaseOrderHeaderDto
    {
        public int OrdKy { get; set; }
        public int OrdNo { get; set; }
        public string? DocNo { get; set; }
        public DateTime OrdDt { get; set; }
        public string? Des { get; set; }
        public int? AccKy { get; set; }
        public string? OrdFrq { get; set; }
        public int? AdrKy { get; set; }
    }
}
