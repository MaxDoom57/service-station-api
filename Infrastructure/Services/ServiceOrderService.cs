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
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey) ?? 0;

                // 1. Handle Vehicle (Update Mileage/Damage)
                // Assuming Vehicle Exists? Prompt says "create... with vehicle details". 
                // If it doesn't exist, we should probably fail or auto-create (but validation is better).
                // I'll search for it.
                var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId && v.fInAct != true);
                if (vehicle == null) return (false, "Vehicle not found", 0);

                // Update Vehicle Info
                vehicle.CurrentMileage = dto.CurrentMileage;
                vehicle.MileageUpdateDtm = DateTime.Now;
                // Damage note is stored in ServiceOrder primarily, but could be on Vehicle description? 
                // Requirement says "in serviceOrder with given details". So DamageNote goes to ServiceOrder.

                // 3. Handle Customer
                // We have vehicle.OwnerAccountKy. We can check if it matches provided Name? 
                // Or just trust the Vehicle Owner Link. 
                // Prompt: "create serviceOrder with customer details(name, id...)"
                // I'll link to the Vehicle's Owner Account.
                if (!vehicle.OwnerAccountKy.HasValue) return (false, "Vehicle has no linked owner", 0);
                int accKy = vehicle.OwnerAccountKy.Value;

                // Validate Package Key
                if (!await db.CdMas.AnyAsync(c => c.CdKy == dto.PackageKy))
                    return (false, "Invalid Package Key", 0);

                // 3. Create ServiceOrder Master
                var order = new ServiceOrder
                {
                    ServiceOrdNo = "SO-" + DateTime.Now.Ticks.ToString().Substring(10), // Simple ID
                    VehicleKy = vehicle.VehicleKy,
                    AccKy = accKy,
                    PackageKy = dto.PackageKy,
                    BayKy = dto.BayKy,
                    CurrentMileage = dto.CurrentMileage,
                    DamageNote = dto.DamageNote,
                    Remarks = dto.AdditionalNotes,
                    Status = "Wait", // Initial State
                    fInAct = false,
                    EntUsrKy = userKey,
                    EntDtm = DateTime.Now,
                    CKy = _userContext.CompanyKey
                };

                db.ServiceOrder.Add(order);
                await db.SaveChangesAsync();

                // 4. Add Package Items to Details
                // Fetch items for package
                var pkgItems = await db.ItmMas
                    .Where(x => x.ItmTypKy == dto.PackageKy && !x.fInAct)
                    .ToListAsync();

                foreach (var item in pkgItems)
                {
                    var detail = new ServiceOrderDetail
                    {
                        ServiceOrdKy = order.ServiceOrdKy,
                        ItemKy = item.ItmKy,
                        ItemName = item.ItmNm,
                        Price = item.SlsPri, // Base price
                        EstimatedTime = "Standard", // Or fetch from somewhere if available
                        StatusWait = 1, // Start as Wait
                        StatusInProgress = 0,
                        StatusFinish = 0,
                        IsApproved = true, // Package items are approved
                        EntUsrKy = userKey,
                        EntDtm = DateTime.Now
                    };
                    db.ServiceOrderDetail.Add(detail);
                }

                await db.SaveChangesAsync();

                // Sync to OrdMas/OrdDet
                await CreateOrdMasAndDetSync(db, order, userKey);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, "Service Order Created", order.ServiceOrdKy);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error: " + ex.Message, 0);
            }
        }

        public async Task<(bool success, string message)> AddServiceItemAsync(AddServiceItemDto dto)
        {
             using var db = await _factory.CreateDbContextAsync();
             var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey) ?? 0;

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
                     EntDtm = DateTime.Now
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
                    ApprovedDtm = DateTime.Now
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

                // Update Master Status Logic
                await UpdateMasterStatus(db, item.ServiceOrdKy);

                return (true, "Status updated");
            }
            catch (Exception ex)
            {
                return (false, "Error updating status: " + ex.Message);
            }
        }

        private async Task UpdateMasterStatus(DynamicDbContext db, int ordKy)
        {
            var order = await db.ServiceOrder.FindAsync(ordKy);
            if (order == null) return; // Handle orphan items gracefully

            var items = await db.ServiceOrderDetail.Where(x => x.ServiceOrdKy == ordKy).ToListAsync();

            // Logic:
            // if no items is under inprogress or finish, service order state is wait. (All Wait)
            // if any service item states is progress or finish, service state is ongoing. 
            // if whole items have finish state, serviceOrder state is finish.

            bool anyProgress = items.Any(i => i.StatusInProgress == 1);
            bool anyFinish = items.Any(i => i.StatusFinish == 1);
            bool allFinish = items.All(i => i.StatusFinish == 1);
            bool allWait = items.All(i => i.StatusWait == 1);

            if (allFinish)
            {
                order.Status = "Finish";
            }
            else if (anyProgress || anyFinish) // If any is finish but NOT ALL, it's still ongoing logically? Prompt: "if any ... is progress or finish, service state is ongoing". Yes. Because "whole items have finish state" is the ONLY condition for Master Finish. So partial finish = Ongoing.
            {
                order.Status = "InProgress";
            }
            else
            {
                order.Status = "Wait";
            }

            await db.SaveChangesAsync();
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
                EntDtm = DateTime.Now,
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
                EntDtm = DateTime.Now,
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
