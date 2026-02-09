using System;
using System.Collections.Generic;

namespace Application.DTOs.Order
{
    public class CreateOrderDto
    {
        // Order Master Fields
        public short LocKy { get; set; }
        public string? OrdTyp { get; set; }
        public short OrdTypKy { get; set; }
        public int Adrky { get; set; }
        public int? AccKy { get; set; }
        public short PmtTrmKy { get; set; }
        public decimal SlsPri { get; set; }
        public float? DisPer { get; set; }
        public string? Des { get; set; }
        public string? DocNo { get; set; }
        public string? YurRef { get; set; }
        public DateTime? OrdDt { get; set; }
        public DateTime? DlryDt { get; set; }
        public short OrdFrqKy { get; set; }
        public short OrdStsKy { get; set; }
        public int? RepAdrKy { get; set; }
        public int? DistAdrKy { get; set; }
        public short BUKy { get; set; }
        public short OrdCat1Ky { get; set; }
        public short OrdCat2Ky { get; set; }
        public short OrdCat3Ky { get; set; }
        public decimal? Amt1 { get; set; }
        public decimal? Amt2 { get; set; }
        public float MarPer { get; set; }
        public string? Status { get; set; }
        public short SKy { get; set; }
        public string? OrdRem { get; set; }

        // Order Details
        public List<CreateOrderDetailDto> OrderDetails { get; set; } = new();
    }

    public class CreateOrderDetailDto
    {
        public int AdrKy { get; set; }
        public double LiNo { get; set; }
        public int? ItmKy { get; set; }
        public string? ItmCd { get; set; }
        public string? Des { get; set; }
        public string? Status { get; set; }
        public double? EstQty { get; set; }
        public double? OrdQty { get; set; }
        public double? DlvQty { get; set; }
        public double BulkQty { get; set; }
        public decimal EstPri { get; set; }
        public decimal OrdPri { get; set; }
        public decimal? CosPri { get; set; }
        public decimal? SlsPri { get; set; }
        public float DisPer { get; set; }
        public decimal DisAmt { get; set; }
        public byte? fApr { get; set; }
        public bool fVirtItm { get; set; }
        public string? OrdItmCd { get; set; }
        public short? OrdStsKy { get; set; }
        public short OrdUnitKy { get; set; }
        public double BulkFctr { get; set; }
        public DateTime? ReqDt { get; set; }
        public short? BulkUnitKy { get; set; }
        public int StkStsKy { get; set; }
        public decimal? GrsWt { get; set; }
        public decimal NetWt { get; set; }
        public short BUKy { get; set; }
        public short CdKy1 { get; set; }
        public decimal Amt1 { get; set; }
        public decimal Amt2 { get; set; }
        public bool fNoPrnPri { get; set; }
        public byte? FmtFntSize { get; set; }
        public bool? FmtFntUndLn { get; set; }
        public byte? FmtFntStyle { get; set; }
        public short? Cd2Ky { get; set; }
        public short? Cd3Ky { get; set; }
        public bool? FmtPrtPri { get; set; }
        public string? Rem { get; set; }
        public string? SpecCd { get; set; }
        public bool isMatSub { get; set; }
        public bool isLabSub { get; set; }
        public bool isPltSub { get; set; }
        public decimal MatAmt { get; set; }
        public decimal LabAmt { get; set; }
        public decimal PltAmt { get; set; }
        public decimal SubConAmt { get; set; }
        public short SubOHP { get; set; }
    }

    public class UpdateOrderDto : CreateOrderDto
    {
        public int OrdKy { get; set; }
    }

    public class OrderListDto
    {
        public int OrdKy { get; set; }
        public int? OrdNo { get; set; }
        public string? OrdTyp { get; set; }
        public DateTime? OrdDt { get; set; }
        public string? Des { get; set; }
        public decimal SlsPri { get; set; }
        public string? Status { get; set; }
        public bool fFinish { get; set; }
        public string? CustomerName { get; set; }
    }

    public class OrderDetailResponseDto
    {
        public int OrdKy { get; set; }
        public int? OrdNo { get; set; }
        public short CKy { get; set; }
        public short LocKy { get; set; }
        public string? OrdTyp { get; set; }
        public short OrdTypKy { get; set; }
        public int Adrky { get; set; }
        public int? AccKy { get; set; }
        public short PmtTrmKy { get; set; }
        public decimal SlsPri { get; set; }
        public float? DisPer { get; set; }
        public bool fInAct { get; set; }
        public byte fApr { get; set; }
        public bool fInv { get; set; }
        public bool fFinish { get; set; }
        public string? Des { get; set; }
        public string? DocNo { get; set; }
        public string? YurRef { get; set; }
        public DateTime? OrdDt { get; set; }
        public DateTime? OrdFinDt { get; set; }
        public DateTime? DlryDt { get; set; }
        public DateTime? EntDtm { get; set; }
        public string? Status { get; set; }
        public string? OrdRem { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }

        public List<OrderDetailItemDto> Details { get; set; } = new();
    }

    public class OrderDetailItemDto
    {
        public int OrdDetKy { get; set; }
        public double LiNo { get; set; }
        public int? ItmKy { get; set; }
        public string? ItmCd { get; set; }
        public string? Des { get; set; }
        public string? Status { get; set; }
        public double? OrdQty { get; set; }
        public double? DlvQty { get; set; }
        public decimal OrdPri { get; set; }
        public decimal? SlsPri { get; set; }
        public float DisPer { get; set; }
        public decimal DisAmt { get; set; }
        public string? Rem { get; set; }
    }
}
