using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reports
{
    public class vewStkCurQtyRpt
    {
        public short? LocKy { get; set; }

        public double? Qty { get; set; }

        public string ItmCd { get; set; } = null!;

        public string? ItmNm { get; set; }

        public string? Unit { get; set; }

        public int ItmKy { get; set; }

        public decimal ItmCosPri { get; set; }

        public string? ItmCat1Nm { get; set; }

        public string? ItmCat2Nm { get; set; }

        public string? ItmCat3Nm { get; set; }

        public string? PartNo { get; set; }

        public string? ItmCat1Cd { get; set; }

        public string? ItmCat2Cd { get; set; }

        public string? ItmCat3Cd { get; set; }

        public decimal ItmSlsPri { get; set; }

        public int CKy { get; set; }

        public string? ItmType { get; set; }

        public short ItmTypKy { get; set; }

        public string? LocCd { get; set; }

        public short ItmCat1Ky { get; set; }

        public short ItmCat2Ky { get; set; }

        public short ItmCat3Ky { get; set; }

        public short ItmCat4Ky { get; set; }

        public short BUKy { get; set; }

        public string? ItmCat4Cd { get; set; }

        public string? ItmCat4Nm { get; set; }

        public string? LocNm { get; set; }

        public string? BUCd { get; set; }

        public string? BUNm { get; set; }
    }
}
