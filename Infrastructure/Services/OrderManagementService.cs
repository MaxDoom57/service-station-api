using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class OrderManagementService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public OrderManagementService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<(bool success, string message, int ordKy)> CreateOrderAsync(CreateOrderDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey) ?? 0;

                // Generate Order Number
                var maxOrdNo = await db.OrdMas
                    .Where(o => o.CKy == _userContext.CompanyKey)
                    .MaxAsync(o => (int?)o.OrdNo) ?? 0;

                var order = new OrdMas
                {
                    CKy = (short)_userContext.CompanyKey,
                    LocKy = dto.LocKy,
                    OrdNo = maxOrdNo + 1,
                    OrdTyp = dto.OrdTyp,
                    OrdTypKy = dto.OrdTypKy,
                    Adrky = dto.Adrky,
                    AccKy = dto.AccKy,
                    PmtTrmKy = dto.PmtTrmKy,
                    SlsPri = dto.SlsPri,
                    DisPer = dto.DisPer,
                    fInAct = false,
                    fApr = 0,
                    fInv = false,
                    fFinish = false,
                    Des = dto.Des,
                    DocNo = dto.DocNo,
                    YurRef = dto.YurRef,
                    EntUsrKy = userKey,
                    OrdDt = dto.OrdDt ?? DateTime.Now,
                    DlryDt = dto.DlryDt,
                    EntDtm = DateTime.Now,
                    OrdFrqKy = dto.OrdFrqKy,
                    OrdStsKy = dto.OrdStsKy,
                    OrdRelKy = 0,
                    PrntKy = 0,
                    CusItmKy = 0,
                    RepAdrKy = dto.RepAdrKy,
                    DistAdrKy = dto.DistAdrKy,
                    BUKy = dto.BUKy,
                    OrdCat1Ky = dto.OrdCat1Ky,
                    OrdCat2Ky = dto.OrdCat2Ky,
                    OrdCat3Ky = dto.OrdCat3Ky,
                    Amt1 = dto.Amt1,
                    Amt2 = dto.Amt2,
                    MarPer = dto.MarPer,
                    tOrdSetOff = 0,
                    Status = dto.Status,
                    SKy = dto.SKy,
                    OrdRem = dto.OrdRem
                };

                db.OrdMas.Add(order);
                await db.SaveChangesAsync();

                // Add Order Details
                foreach (var detailDto in dto.OrderDetails)
                {
                    var detail = new OrdDet
                    {
                        Ordky = order.OrdKy,
                        AdrKy = detailDto.AdrKy,
                        LiNo = detailDto.LiNo,
                        ItmKy = detailDto.ItmKy,
                        ItmCd = detailDto.ItmCd,
                        Des = detailDto.Des,
                        Status = detailDto.Status,
                        EstQty = detailDto.EstQty,
                        OrdQty = detailDto.OrdQty,
                        DlvQty = detailDto.DlvQty,
                        BulkQty = detailDto.BulkQty,
                        EstPri = detailDto.EstPri,
                        OrdPri = detailDto.OrdPri,
                        CosPri = detailDto.CosPri,
                        SlsPri = detailDto.SlsPri,
                        DisPer = detailDto.DisPer,
                        DisAmt = detailDto.DisAmt,
                        fApr = detailDto.fApr,
                        fVirtItm = detailDto.fVirtItm,
                        OrdItmCd = detailDto.OrdItmCd,
                        OrdStsKy = detailDto.OrdStsKy,
                        OrdUnitKy = detailDto.OrdUnitKy,
                        BulkFctr = detailDto.BulkFctr,
                        ReqDt = detailDto.ReqDt,
                        BulkUnitKy = detailDto.BulkUnitKy,
                        StkStsKy = detailDto.StkStsKy,
                        GrsWt = detailDto.GrsWt,
                        NetWt = detailDto.NetWt,
                        BUKy = detailDto.BUKy,
                        CdKy1 = detailDto.CdKy1,
                        Amt1 = detailDto.Amt1,
                        Amt2 = detailDto.Amt2,
                        fNoPrnPri = detailDto.fNoPrnPri,
                        EntUsrKy = userKey,
                        EntDtm = DateTime.Now,
                        FmtFntSize = detailDto.FmtFntSize,
                        FmtFntUndLn = detailDto.FmtFntUndLn,
                        FmtFntStyle = detailDto.FmtFntStyle,
                        Cd2Ky = detailDto.Cd2Ky,
                        Cd3Ky = detailDto.Cd3Ky,
                        FmtPrtPri = detailDto.FmtPrtPri,
                        Rem = detailDto.Rem,
                        SpecCd = detailDto.SpecCd,
                        isMatSub = detailDto.isMatSub,
                        isLabSub = detailDto.isLabSub,
                        isPltSub = detailDto.isPltSub,
                        MatAmt = detailDto.MatAmt,
                        LabAmt = detailDto.LabAmt,
                        PltAmt = detailDto.PltAmt,
                        SubConAmt = detailDto.SubConAmt,
                        SubOHP = detailDto.SubOHP
                    };

                    db.OrdDet.Add(detail);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Order created successfully", order.OrdKy);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> UpdateOrderAsync(UpdateOrderDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var order = await db.OrdMas.FindAsync(dto.OrdKy);
                if (order == null || order.fInAct)
                    return (false, "Order not found");

                if (order.CKy != _userContext.CompanyKey)
                    return (false, "Unauthorized");

                // Update Order Master
                order.LocKy = dto.LocKy;
                order.OrdTyp = dto.OrdTyp;
                order.OrdTypKy = dto.OrdTypKy;
                order.Adrky = dto.Adrky;
                order.AccKy = dto.AccKy;
                order.PmtTrmKy = dto.PmtTrmKy;
                order.SlsPri = dto.SlsPri;
                order.DisPer = dto.DisPer;
                order.Des = dto.Des;
                order.DocNo = dto.DocNo;
                order.YurRef = dto.YurRef;
                order.OrdDt = dto.OrdDt;
                order.DlryDt = dto.DlryDt;
                order.OrdFrqKy = dto.OrdFrqKy;
                order.OrdStsKy = dto.OrdStsKy;
                order.RepAdrKy = dto.RepAdrKy;
                order.DistAdrKy = dto.DistAdrKy;
                order.BUKy = dto.BUKy;
                order.OrdCat1Ky = dto.OrdCat1Ky;
                order.OrdCat2Ky = dto.OrdCat2Ky;
                order.OrdCat3Ky = dto.OrdCat3Ky;
                order.Amt1 = dto.Amt1;
                order.Amt2 = dto.Amt2;
                order.MarPer = dto.MarPer;
                order.Status = dto.Status;
                order.SKy = dto.SKy;
                order.OrdRem = dto.OrdRem;

                // Delete existing details
                var existingDetails = await db.OrdDet.Where(d => d.Ordky == dto.OrdKy).ToListAsync();
                db.OrdDet.RemoveRange(existingDetails);

                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey) ?? 0;

                // Add new details
                foreach (var detailDto in dto.OrderDetails)
                {
                    var detail = new OrdDet
                    {
                        Ordky = order.OrdKy,
                        AdrKy = detailDto.AdrKy,
                        LiNo = detailDto.LiNo,
                        ItmKy = detailDto.ItmKy,
                        ItmCd = detailDto.ItmCd,
                        Des = detailDto.Des,
                        Status = detailDto.Status,
                        EstQty = detailDto.EstQty,
                        OrdQty = detailDto.OrdQty,
                        DlvQty = detailDto.DlvQty,
                        BulkQty = detailDto.BulkQty,
                        EstPri = detailDto.EstPri,
                        OrdPri = detailDto.OrdPri,
                        CosPri = detailDto.CosPri,
                        SlsPri = detailDto.SlsPri,
                        DisPer = detailDto.DisPer,
                        DisAmt = detailDto.DisAmt,
                        fApr = detailDto.fApr,
                        fVirtItm = detailDto.fVirtItm,
                        OrdItmCd = detailDto.OrdItmCd,
                        OrdStsKy = detailDto.OrdStsKy,
                        OrdUnitKy = detailDto.OrdUnitKy,
                        BulkFctr = detailDto.BulkFctr,
                        ReqDt = detailDto.ReqDt,
                        BulkUnitKy = detailDto.BulkUnitKy,
                        StkStsKy = detailDto.StkStsKy,
                        GrsWt = detailDto.GrsWt,
                        NetWt = detailDto.NetWt,
                        BUKy = detailDto.BUKy,
                        CdKy1 = detailDto.CdKy1,
                        Amt1 = detailDto.Amt1,
                        Amt2 = detailDto.Amt2,
                        fNoPrnPri = detailDto.fNoPrnPri,
                        EntUsrKy = userKey,
                        EntDtm = DateTime.Now,
                        FmtFntSize = detailDto.FmtFntSize,
                        FmtFntUndLn = detailDto.FmtFntUndLn,
                        FmtFntStyle = detailDto.FmtFntStyle,
                        Cd2Ky = detailDto.Cd2Ky,
                        Cd3Ky = detailDto.Cd3Ky,
                        FmtPrtPri = detailDto.FmtPrtPri,
                        Rem = detailDto.Rem,
                        SpecCd = detailDto.SpecCd,
                        isMatSub = detailDto.isMatSub,
                        isLabSub = detailDto.isLabSub,
                        isPltSub = detailDto.isPltSub,
                        MatAmt = detailDto.MatAmt,
                        LabAmt = detailDto.LabAmt,
                        PltAmt = detailDto.PltAmt,
                        SubConAmt = detailDto.SubConAmt,
                        SubOHP = detailDto.SubOHP
                    };

                    db.OrdDet.Add(detail);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Order updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteOrderAsync(int ordKy)
        {
            using var db = await _factory.CreateDbContextAsync();
            
            var order = await db.OrdMas.FindAsync(ordKy);
            if (order == null)
                return (false, "Order not found");

            if (order.CKy != _userContext.CompanyKey)
                return (false, "Unauthorized");

            order.fInAct = true;
            await db.SaveChangesAsync();

            return (true, "Order deleted successfully");
        }

        public async Task<List<OrderListDto>> GetAllOrdersAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            var orders = await (from o in db.OrdMas
                               join a in db.Account on o.AccKy equals a.AccKy into accGroup
                               from a in accGroup.DefaultIfEmpty()
                               where o.CKy == _userContext.CompanyKey && !o.fInAct
                               orderby o.OrdDt descending
                               select new OrderListDto
                               {
                                   OrdKy = o.OrdKy,
                                   OrdNo = o.OrdNo,
                                   OrdTyp = o.OrdTyp,
                                   OrdDt = o.OrdDt,
                                   Des = o.Des,
                                   SlsPri = o.SlsPri,
                                   Status = o.Status,
                                   fFinish = o.fFinish,
                                   CustomerName = a != null ? a.AccNm : ""
                               }).ToListAsync();

            return orders;
        }

        public async Task<OrderDetailResponseDto?> GetOrderByOrderNoAsync(int ordNo)
        {
            using var db = await _factory.CreateDbContextAsync();

            var order = await db.OrdMas
                .FirstOrDefaultAsync(o => o.OrdNo == ordNo && o.CKy == _userContext.CompanyKey && !o.fInAct);

            if (order == null)
                return null;

            return await BuildOrderDetailResponse(db, order);
        }

        public async Task<OrderDetailResponseDto?> GetOrderByKeyAsync(int ordKy)
        {
            using var db = await _factory.CreateDbContextAsync();

            var order = await db.OrdMas
                .FirstOrDefaultAsync(o => o.OrdKy == ordKy && o.CKy == _userContext.CompanyKey && !o.fInAct);

            if (order == null)
                return null;

            return await BuildOrderDetailResponse(db, order);
        }

        private async Task<OrderDetailResponseDto> BuildOrderDetailResponse(DynamicDbContext db, OrdMas order)
        {
            var account = await db.Account.FindAsync(order.AccKy);
            var address = await db.Addresses.FindAsync(order.Adrky);

            var details = await db.OrdDet
                .Where(d => d.Ordky == order.OrdKy)
                .OrderBy(d => d.LiNo)
                .Select(d => new OrderDetailItemDto
                {
                    OrdDetKy = d.OrdDetKy,
                    LiNo = d.LiNo,
                    ItmKy = d.ItmKy,
                    ItmCd = d.ItmCd,
                    Des = d.Des,
                    Status = d.Status,
                    OrdQty = d.OrdQty,
                    DlvQty = d.DlvQty,
                    OrdPri = d.OrdPri,
                    SlsPri = d.SlsPri,
                    DisPer = d.DisPer,
                    DisAmt = d.DisAmt,
                    Rem = d.Rem
                })
                .ToListAsync();

            return new OrderDetailResponseDto
            {
                OrdKy = order.OrdKy,
                OrdNo = order.OrdNo,
                CKy = order.CKy,
                LocKy = order.LocKy,
                OrdTyp = order.OrdTyp,
                OrdTypKy = order.OrdTypKy,
                Adrky = order.Adrky,
                AccKy = order.AccKy,
                PmtTrmKy = order.PmtTrmKy,
                SlsPri = order.SlsPri,
                DisPer = order.DisPer,
                fInAct = order.fInAct,
                fApr = order.fApr,
                fInv = order.fInv,
                fFinish = order.fFinish,
                Des = order.Des,
                DocNo = order.DocNo,
                YurRef = order.YurRef,
                OrdDt = order.OrdDt,
                OrdFinDt = order.OrdFinDt,
                DlryDt = order.DlryDt,
                EntDtm = order.EntDtm,
                Status = order.Status,
                OrdRem = order.OrdRem,
                CustomerName = account?.AccNm,
                CustomerAddress = address?.Address,
                Details = details
            };
        }
    }
}
