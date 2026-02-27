using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Reports
{
    public class vewSlsDtls
    {
        public int? ItmKy { get; set; }
        public string? ItmCd { get; set; }
        public string? ItmNm { get; set; }
        public string? Unit { get; set; }

        public decimal? CosPri { get; set; }
        public decimal? SlsPri { get; set; }
        public decimal? TrnPri { get; set; }

        public double? DisPer { get; set; }

        public double? Qty { get; set; }
        public double? Qty2 { get; set; }

        public int? ItmTrnKy { get; set; }
        public int UpDt { get; set; }

        public short? LiNo { get; set; }
        public int TrnNo { get; set; }

        public int RepAdrKy { get; set; }
        public string? SlsRepAdrCd { get; set; }
        public string? SlsRepAdrNm { get; set; }

        public decimal? DisAmt { get; set; }
        public DateTime TrnDt { get; set; }

        public short PmtTrmKy { get; set; }
        public string? PmtTrmCd { get; set; }
        public string? PmtTrmNm { get; set; }

        public string? AccCd { get; set; }
        public string? AccNm { get; set; }
        public int AccKy { get; set; }

        public string? CustAdrCd { get; set; }
        public string? CustAdrNm { get; set; }
        public int Adrky { get; set; }

        public string? OurCd { get; set; }

        public short LocKy { get; set; }
        public string? LocCd { get; set; }
        public string? LocNm { get; set; }

        public short? ItmCat1Ky { get; set; }
        public short? ItmCat2Ky { get; set; }
        public short? ItmCat3Ky { get; set; }

        public string? ItmCat1Nm { get; set; }
        public string? ItmCat1Cd { get; set; }

        public int? EntUsrKy { get; set; }

        public short? BUKy { get; set; }
        public string? BUCd { get; set; }
        public string? BUNm { get; set; }
    }
}
