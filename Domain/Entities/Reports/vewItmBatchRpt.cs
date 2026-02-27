using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reports
{
    public class vewItmBatchRpt
    {
        public int ItmKy { get; set; }

        public string ItmCd { get; set; } = null!;

        public string? PartNo { get; set; }

        public string? ItmNm { get; set; }

        public int ItmBatchKy { get; set; }

        public string? BatchNo { get; set; }

        public DateTime? ExpirDt { get; set; }

        public float CosPri { get; set; }

        public float SalePri { get; set; }

        public float Qty { get; set; }

        public double? ItmLocQty { get; set; }
    }
}
