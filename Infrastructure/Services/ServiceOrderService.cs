using Application.DTOs.ServiceOrder;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class ServiceOrderService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;
        private readonly VehicleService _vehicleService;

        public ServiceOrderService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService,
            VehicleService vehicleService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
            _vehicleService = vehicleService;
        }

        public async Task<(bool success, string message, int ordKy)> CreateServiceOrderAsync(CreateServiceOrderDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();

            var result = (success: false, message: string.Empty, ordKy: 0);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, 1) ?? 0;

                    // 1. Find Vehicle
                    var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId && v.fInAct != true);
                    if (vehicle == null)
                    {
                        result = (false, "Vehicle not found", 0);
                        return;
                    }

                    // Update Vehicle Mileage
                    vehicle.CurrentMileage = dto.CurrentMileage;
                    vehicle.MileageUpdateDtm = AppTime.Now;

                    // 2. Resolve Customer Account via Vehicle Owner
                    if (!vehicle.OwnerAccountKy.HasValue)
                    {
                        result = (false, "Vehicle has no linked owner", 0);
                        return;
                    }
                    int accKy = vehicle.OwnerAccountKy.Value;

                    // 3. Validate Package Key
                    if (!await db.CdMas.AnyAsync(c => c.CdKy == dto.PackageKy))
                    {
                        result = (false, "Invalid Package Key", 0);
                        return;
                    }

                    // 4. Create ServiceOrder Master
                    var order = new ServiceOrder
                    {
                        ServiceOrdNo = "SO-" + AppTime.Now.Ticks.ToString().Substring(10),
                        VehicleKy = vehicle.VehicleKy,
                        AccKy = accKy,
                        PackageKy = dto.PackageKy,
                        BayKy = dto.BayKy,
                        CurrentMileage = dto.CurrentMileage,
                        DamageNote = dto.DamageNote,
                        Remarks = dto.AdditionalNotes,
                        Status = "Wait",
                        fInAct = false,
                        EntUsrKy = userKey,
                        EntDtm = AppTime.Now,
                        CKy = _userContext.CompanyKey
                    };

                    db.ServiceOrder.Add(order);
                    await db.SaveChangesAsync();

                    // 5. Add Package Items to Details
                    var pkgItems = await db.ItmMas
                        .Where(x => x.ItmTypKy == dto.PackageKy && !x.fInAct)
                        .ToListAsync();

                    foreach (var item in pkgItems)
                    {
                        db.ServiceOrderDetail.Add(new ServiceOrderDetail
                        {
                            ServiceOrdKy = order.ServiceOrdKy,
                            ItemKy = item.ItmKy,
                            ItemName = item.ItmNm ?? "",
                            Price = item.SlsPri,
                            EstimatedTime = "Standard",
                            StatusWait = 1,
                            StatusInProgress = 0,
                            StatusFinish = 0,
                            IsApproved = true,
                            EntUsrKy = userKey,
                            EntDtm = AppTime.Now
                        });
                    }

                    await db.SaveChangesAsync();

                    // 6. Sync to OrdMas/OrdDet
                    await CreateOrdMasAndDetSync(db, order, userKey);
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();
                    result = (true, "Service Order Created", order.ServiceOrdKy);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result = (false, "Error: " + ex.Message, 0);
                }
            });

            return result;
        }

        public async Task<(bool success, string message)> AddServiceItemAsync(AddServiceItemDto dto)
        {
             using var db = await _factory.CreateDbContextAsync();
             var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, 1) ?? 0;

             // Validate Service Order Exists
             var orderExists = await db.ServiceOrder.AnyAsync(o => o.ServiceOrdKy == dto.ServiceOrdKy);
             if (!orderExists) return (false, "Service Order not found");

             try
             {
                 var item = new ServiceOrderDetail
                 {
                     ServiceOrdKy = dto.ServiceOrdKy,
                     ItemKy = null, // Custom item
                     ItemName = dto.ItemName,
                     Price = dto.Price,
                     EstimatedTime = dto.EstimatedTime,
                     StatusWait = 1,
                     StatusInProgress = 0,
                     StatusFinish = 0,
                     IsApproved = false, // Pending Approval
                     EntUsrKy = userKey,
                     EntDtm = AppTime.Now
                 };

                 db.ServiceOrderDetail.Add(item);
                 await db.SaveChangesAsync();
                 return (true, "Item added, pending approval");
             }
             catch (Exception ex)
             {
                 return (false, "Error adding item: " + ex.Message);
             }
        }

        public async Task<(bool success, string message)> ApproveServiceItemAsync(ApproveServiceItemDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            var item = await db.ServiceOrderDetail.FindAsync(dto.ServiceOrdDetKy);
            if (item == null) return (false, "Item not found");

            try
            {
                if (!dto.IsApproved)
                {
                    db.ServiceOrderDetail.Remove(item); // Or mark inactive? Hard delete for pending items is often okay.
                    await db.SaveChangesAsync();
                    return (true, "Item rejected/removed");
                }

                item.IsApproved = true;

                // Save Approval Info
                var approval = new ServiceOrderApproval
                {
                    ServiceOrdDetKy = dto.ServiceOrdDetKy,
                    CustName = dto.CustName,
                    IpAddress = dto.IpAddress,
                    Device = dto.Device,
                    ApprovedDtm = AppTime.Now
                };
                db.ServiceOrderApproval.Add(approval);
                
                await db.SaveChangesAsync();

                // Sync Sync to OrdDet
                await CreateOrdDetSync(db, item, (od) => db.OrdDet.Add(od));
                await db.SaveChangesAsync();

                return (true, "Item approved");
            }
            catch (Exception ex)
            {
                return (false, "Error approving item: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> UpdateItemStatusAsync(UpdateItemStatusDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            var item = await db.ServiceOrderDetail.FindAsync(dto.ServiceOrdDetKy);
            if (item == null) return (false, "Item not found");

            try
            {
                // Reset
                item.StatusWait = 0;
                item.StatusInProgress = 0;
                item.StatusFinish = 0;

                if (dto.Status == "Wait") item.StatusWait = 1;
                else if (dto.Status == "InProgress") item.StatusInProgress = 1;
                else if (dto.Status == "Finish") item.StatusFinish = 1;
                else return (false, "Invalid status");

                await db.SaveChangesAsync();

                // Update Master Status Logic is removed as it's now manually controlled
                // await UpdateMasterStatus(db, item.ServiceOrdKy);

                return (true, "Status updated");
            }
            catch (Exception ex)
            {
                return (false, "Error updating status: " + ex.Message);
            }
        }

        // private async Task UpdateMasterStatus(DynamicDbContext db, int ordKy) removed, now manually handled

        public async Task<(bool success, string message)> UpdateServiceOrderStatusAsync(UpdateServiceOrderStatusDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            var order = await db.ServiceOrder.FindAsync(dto.ServiceOrdKy);
            if (order == null) return (false, "Service Order not found");

            try
            {
                order.Status = dto.Status;
                await db.SaveChangesAsync();
                return (true, "Service Order status updated");
            }
            catch (Exception ex)
            {
                return (false, "Error updating status: " + ex.Message);
            }
        }

        public async Task<List<ServiceOrderDto>> GetServiceOrdersAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            var orders = await db.ServiceOrder
                .Where(x => !x.fInAct)
                .ToListAsync();

            var result = new List<ServiceOrderDto>();

            foreach(var o in orders)
            {
                var v = await db.Vehicles.FindAsync(o.VehicleKy);
                var c = await db.Account.FindAsync(o.AccKy);
                var p = await db.CdMas.FindAsync((short)o.PackageKy);
                var items = await db.ServiceOrderDetail.Where(d => d.ServiceOrdKy == o.ServiceOrdKy).ToListAsync();

                result.Add(new ServiceOrderDto
                {
                    ServiceOrdKy = o.ServiceOrdKy,
                    ServiceOrdNo = o.ServiceOrdNo,
                    Date = o.EntDtm,
                    VehicleId = v?.VehicleId ?? "",
                    CustomerName = c?.AccNm ?? "",
                    PackageName = p?.CdNm ?? "",
                    Status = o.Status,
                    Items = items.Select(i => new ServiceOrderDetailDto
                    {
                        ServiceOrdDetKy = i.ServiceOrdDetKy,
                        ItemName = i.ItemName,
                        Price = i.Price,
                        IsApproved = i.IsApproved,
                        Status = i.StatusInProgress == 1 ? "InProgress" : (i.StatusFinish == 1 ? "Finish" : "Wait")
                    }).ToList()
                });
            }
            return result;
        }

        public async Task<ServiceOrderDto?> GetServiceOrderDetailsAsync(int ordKy)
        {
            using var db = await _factory.CreateDbContextAsync();
             var o = await db.ServiceOrder.FirstOrDefaultAsync(x => x.ServiceOrdKy == ordKy);
             if (o == null) return null;

                var v = await db.Vehicles.FindAsync(o.VehicleKy);
                var c = await db.Account.FindAsync(o.AccKy);
                var p = await db.CdMas.FindAsync((short)o.PackageKy);
                var items = await db.ServiceOrderDetail.Where(d => d.ServiceOrdKy == o.ServiceOrdKy).ToListAsync();

                return new ServiceOrderDto
                {
                    ServiceOrdKy = o.ServiceOrdKy,
                    ServiceOrdNo = o.ServiceOrdNo,
                    Date = o.EntDtm,
                    VehicleId = v?.VehicleId ?? "",
                    CustomerName = c?.AccNm ?? "",
                    PackageName = p?.CdNm ?? "",
                    Status = o.Status,
                    Items = items.Select(i => new ServiceOrderDetailDto
                    {
                        ServiceOrdDetKy = i.ServiceOrdDetKy,
                        ItemName = i.ItemName,
                        Price = i.Price,
                        IsApproved = i.IsApproved,
                        Status = i.StatusInProgress == 1 ? "InProgress" : (i.StatusFinish == 1 ? "Finish" : "Wait")
                    }).ToList()
                };
        }

        public async Task<ServiceOrderDto?> GetServiceOrderDetailsByVehicleIdAsync(string vehicleId)
        {
            using var db = await _factory.CreateDbContextAsync();
            var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId && v.fInAct != true);
            if (vehicle == null) return null;

            var o = await db.ServiceOrder
                .Where(x => x.VehicleKy == vehicle.VehicleKy && !x.fInAct)
                .OrderByDescending(x => x.EntDtm)
                .FirstOrDefaultAsync();

            if (o == null) return null;

            var c = await db.Account.FindAsync(o.AccKy);
            var p = await db.CdMas.FindAsync((short)o.PackageKy);
            var items = await db.ServiceOrderDetail.Where(d => d.ServiceOrdKy == o.ServiceOrdKy).ToListAsync();

            return new ServiceOrderDto
            {
                ServiceOrdKy = o.ServiceOrdKy,
                ServiceOrdNo = o.ServiceOrdNo,
                Date = o.EntDtm,
                VehicleId = vehicle.VehicleId,
                CustomerName = c?.AccNm ?? "",
                PackageName = p?.CdNm ?? "",
                Status = o.Status,
                Items = items.Select(i => new ServiceOrderDetailDto
                {
                    ServiceOrdDetKy = i.ServiceOrdDetKy,
                    ItemName = i.ItemName,
                    Price = i.Price,
                    IsApproved = i.IsApproved,
                    Status = i.StatusInProgress == 1 ? "InProgress" : (i.StatusFinish == 1 ? "Finish" : "Wait")
                }).ToList()
            };
        }
        // ---------------------------------------------------------
        // Sync Logic for OrdMas / OrdDet
        // ---------------------------------------------------------

        private async Task CreateOrdMasAndDetSync(DynamicDbContext db, ServiceOrder so, int userKey)
        {
            // 1. Resolve Address
            var accAdr = await db.AccAdr.FirstOrDefaultAsync(x => x.AccKy == so.AccKy);
            int adrKy = accAdr?.AdrKy ?? 1;

            // 2. Resolve OrdTypKy ("SLSORD")
            var ordTypCd = await db.CdMas.FirstOrDefaultAsync(c => c.Code == "SLSORD" && c.ConCd == "OrdTyp");
            short ordTypKy = (short)(ordTypCd?.CdKy ?? 1);

            // 3. Get Next OrdNo from OrdNoLst (NOT TrnNoLst)
            int nextOrdNo = 1;
            // Use OrdNoLst table as requested
            var ordNoLst = await db.OrdNoLst.FirstOrDefaultAsync(x => x.OurCd == "SLSORD");
            if (ordNoLst != null)
            {
                nextOrdNo = ordNoLst.LstOrdNo + 1;
                ordNoLst.LstOrdNo = nextOrdNo; // Update Last No
            }
            else
            {
                // Optional: Create new entry or fallback
                // If the user provided the table data, it assumes it exists. 
                // We'll proceed with 1 if not found.
            }

            // 4. Create OrdMas
            var ordMas = new OrdMas
            {
                CKy = (short)so.CKy,
                LocKy = 1,
                OrdNo = nextOrdNo,
                OrdTyp = "SLSORD",
                OrdTypKy = ordTypKy,
                Adrky = adrKy,
                AccKy = so.AccKy,
                PmtTrmKy = 1,
                SlsPri = 0, 
                fInAct = false,
                fApr = 1,
                fInv = false,
                fFinish = false,
                Des = (!string.IsNullOrEmpty(so.Remarks) ? so.Remarks : "Service Order " + so.ServiceOrdNo),
                DocNo = so.ServiceOrdNo, // Link to SO
                YurRef = so.ServiceOrdNo,
                EntUsrKy = userKey,
                OrdDt = so.EntDtm,
                EntDtm = AppTime.Now,
                OrdFrqKy = 1,
                OrdStsKy = 1,
                BUKy = 1,
                SKy = 1
            };

            db.OrdMas.Add(ordMas);
            await db.SaveChangesAsync(); // Generates OrdKy and updates OrdNoLst

            // 5. Create OrdDets
            var details = await db.ServiceOrderDetail.Where(x => x.ServiceOrdKy == so.ServiceOrdKy).ToListAsync();
            double liNo = 1;
            foreach (var d in details)
            {
                await CreateOrdDetLogic(db, d, ordMas.OrdKy, userKey, liNo++);
            }
        }

        private async Task CreateOrdDetSync(DynamicDbContext db, ServiceOrderDetail item, Action<OrdDet> addAction)
        {
            var so = await db.ServiceOrder.FindAsync(item.ServiceOrdKy);
            if (so == null) return;

            var ordMas = await db.OrdMas.FirstOrDefaultAsync(x => x.DocNo == so.ServiceOrdNo && x.OrdTyp == "SLSORD");
            if (ordMas != null)
            {
                await CreateOrdDetLogic(db, item, ordMas.OrdKy, item.EntUsrKy, null);
            }
        }

        private async Task CreateOrdDetLogic(DynamicDbContext db, ServiceOrderDetail item, int ordKy, int userKey, double? explicitLiNo)
        {
            // Resolve Item Codes
            string itmCd = "CUSTOM";
            string des = item.ItemName;
            
            if (item.ItemKy.HasValue)
            {
                var itm = await db.ItmMas.FindAsync(item.ItemKy.Value);
                if (itm != null) itmCd = itm.ItmCd;
            }

            // Resolve LiNo
            double liNoToUse;
            if (explicitLiNo.HasValue)
            {
                liNoToUse = explicitLiNo.Value;
            }
            else
            {
                var maxLiNo = await db.OrdDet.Where(x => x.Ordky == ordKy).MaxAsync(x => (double?)x.LiNo) ?? 0;
                liNoToUse = maxLiNo + 1;
            }

            var det = new OrdDet
            {
                Ordky = ordKy,
                LiNo = liNoToUse,
                ItmKy = item.ItemKy,
                ItmCd = itmCd,
                Des = des,
                Status = "A",
                EstQty = 1,
                OrdQty = 1,
                DlvQty = 0,
                BulkQty = 0,
                EstPri = item.Price,
                OrdPri = item.Price,
                SlsPri = item.Price,
                DisPer = 0,
                DisAmt = 0,
                fApr = 1,
                fVirtItm = false,
                EntUsrKy = userKey,
                EntDtm = AppTime.Now,
                AdrKy = 1, 
                BUKy = 1,
                CdKy1 = 1,
                Amt1 = 0,
                Amt2 = 0,
                MatAmt = 0, LabAmt = 0, PltAmt = 0, SubConAmt = 0
            };

            db.OrdDet.Add(det);
        }
    }
}

