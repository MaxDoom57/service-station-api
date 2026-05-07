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
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ISmsService _smsService;

        public ServiceOrderService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService,
            VehicleService vehicleService,
            ICloudinaryService cloudinaryService,
            ISmsService smsService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
            _vehicleService = vehicleService;
            _cloudinaryService = cloudinaryService;
            _smsService = smsService;
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
                    var userId = !string.IsNullOrEmpty(dto.UserId) ? dto.UserId : _userContext.UserId;
                    var userKey = await _userKeyService.GetUserKeyAsync(userId, 1) ?? 0;

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

                    // 3. Validate Package Key (only if provided)
                    if (dto.PackageKy.HasValue && dto.PackageKy > 0)
                    {
                        if (!await db.CdMas.AnyAsync(c => c.CdKy == dto.PackageKy))
                        {
                            result = (false, "Invalid Package Key", 0);
                            return;
                        }
                    }

                    // 4. Create ServiceOrder Master
                    var order = new ServiceOrder
                    {
                        ServiceOrdNo = "SO-" + AppTime.Now.Ticks.ToString().Substring(10),
                        VehicleKy = vehicle.VehicleKy,
                        AccKy = accKy,
                        PackageKy = dto.PackageKy ?? 0,
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

                    // 5. Add Items to ServiceOrderDetails
                    if (dto.PackageKy.HasValue && dto.PackageKy > 0)
                    {
                        // Package flow: fetch items by package type
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
                    }
                    else
                    {
                        // Manual item flow: use dto.Items
                        foreach (var inputItem in dto.Items!)
                        {
                            var itmMas = await db.ItmMas.FirstOrDefaultAsync(x => x.ItmKy == inputItem.ItmKy && !x.fInAct);
                            if (itmMas == null)
                            {
                                result = (false, $"Item with ItmKy {inputItem.ItmKy} not found or inactive", 0);
                                return;
                            }

                            db.ServiceOrderDetail.Add(new ServiceOrderDetail
                            {
                                ServiceOrdKy = order.ServiceOrdKy,
                                ItemKy = itmMas.ItmKy,
                                ItemName = itmMas.ItmNm ?? "",
                                Price = itmMas.SlsPri,
                                EstimatedTime = inputItem.EstimatedTime ?? "Standard",
                                StatusWait = 1,
                                StatusInProgress = 0,
                                StatusFinish = 0,
                                IsApproved = true,
                                EntUsrKy = userKey,
                                EntDtm = AppTime.Now
                            });
                        }
                    }

                    await db.SaveChangesAsync();

                    // 6. Sync to OrdMas/OrdDet
                    int ordMasKy = await CreateOrdMasAndDetSync(db, order, userKey);
                    
                    // Update ServiceOrder with OrdKy
                    order.OrdKy = ordMasKy;
                    await db.SaveChangesAsync();

                    // 7. Upload Images
                    if (!string.IsNullOrEmpty(dto.SignatureImage))
                    {
                        var uploadResult = await _cloudinaryService.UploadImageAsync(dto.SignatureImage, "service_orders/signatures");
                        if (uploadResult.Url != null)
                        {
                            db.OrdImg.Add(new OrdImg
                            {
                                OrdKy = ordMasKy,
                                ServiceOrdKy = order.ServiceOrdKy,
                                ImageUrl = uploadResult.Url,
                                PublicId = uploadResult.PublicId,
                                ImageType = "Signature",
                                fInAct = false,
                                EntUsrKy = userKey,
                                EntDtm = AppTime.Now
                            });
                        }
                    }

                    if (dto.VehicleImages != null && dto.VehicleImages.Count > 0)
                    {
                        foreach (var img in dto.VehicleImages)
                        {
                            var uploadResult = await _cloudinaryService.UploadImageAsync(img, "service_orders/vehicle_checks");
                            if (uploadResult.Url != null)
                            {
                                db.OrdImg.Add(new OrdImg
                                {
                                    OrdKy = ordMasKy,
                                    ServiceOrdKy = order.ServiceOrdKy,
                                    ImageUrl = uploadResult.Url,
                                    PublicId = uploadResult.PublicId,
                                    ImageType = "Vehicle",
                                    fInAct = false,
                                    EntUsrKy = userKey,
                                    EntDtm = AppTime.Now
                                });
                            }
                        }
                    }

                    // Save image records if any
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();
                    result = (true, "Service Order Created", ordMasKy);

                    // Send SMS: Service Order Created
                    var accAdr = await db.AccAdr.FirstOrDefaultAsync(a => a.AccKy == accKy);
                    if (accAdr != null)
                    {
                        var adr = await db.Addresses.FirstOrDefaultAsync(a => a.AdrKy == accAdr.AdrKy);
                        if (adr?.TP1 != null)
                        {
                            var account = await db.Account.FindAsync(accKy);
                            var customerName = account?.AccNm ?? "Customer";
                            var dateTime = AppTime.Now.ToString("yyyy-MM-dd HH:mm");
                            var sms = $"Dear {customerName},\n" +
                                      $"Your vehicle has been successfully received for service and a service order has been created.\n\n" +
                                      $"Vehicle No: {dto.VehicleId}\n" +
                                      $"Service Order No: {order.ServiceOrdNo}\n" +
                                      $"Date & Time: {dateTime}\n\n" +
                                      $"Our team has started the initial inspection and will keep you informed of any updates.\n\n" +
                                      $"Thank you for choosing us.\n" +
                                      $"Best regards,\n" +
                                      $"HATCS";
                            _ = _smsService.SendAsync(adr.TP1, sms);
                        }
                    }
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
             var userId = !string.IsNullOrEmpty(dto.UserId) ? dto.UserId : _userContext.UserId;
             var userKey = await _userKeyService.GetUserKeyAsync(userId, 1) ?? 0;

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

        public async Task<(bool success, string message)> AddServiceItemDirectAsync(AddServiceItemDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            var userId = !string.IsNullOrEmpty(dto.UserId) ? dto.UserId : _userContext.UserId;
            var userKey = await _userKeyService.GetUserKeyAsync(userId, 1) ?? 0;

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
                    IsApproved = true, // Directly approved
                    EntUsrKy = userKey,
                    EntDtm = AppTime.Now
                };

                db.ServiceOrderDetail.Add(item);
                await db.SaveChangesAsync();

                // Sync to OrdDet immediately
                await CreateOrdDetSync(db, item);
                await db.SaveChangesAsync();

                return (true, "Item added directly");
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
                await CreateOrdDetSync(db, item);
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

                var userId = !string.IsNullOrEmpty(dto.UserId) ? dto.UserId : _userContext.UserId;
                var userKey = await _userKeyService.GetUserKeyAsync(userId, 1) ?? 100;
                item.EntUsrKy = userKey;

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
                
                var userId = !string.IsNullOrEmpty(dto.UserId) ? dto.UserId : _userContext.UserId;
                var userKey = await _userKeyService.GetUserKeyAsync(userId, 1) ?? 0;
                order.EntUsrKy = userKey;

                await db.SaveChangesAsync();

                if (dto.Status == "Finish" && order.OrdKy.HasValue)
                {
                    await SyncToTrnMas(db, order.OrdKy.Value, userKey);

                    // Send SMS: Order Finalized / TrnMas saved
                    var accAdr = await db.AccAdr.FirstOrDefaultAsync(a => a.AccKy == order.AccKy);
                    if (accAdr != null)
                    {
                        var adr = await db.Addresses.FirstOrDefaultAsync(a => a.AdrKy == accAdr.AdrKy);
                        if (adr?.TP1 != null)
                        {
                            var account = await db.Account.FindAsync(order.AccKy);
                            var customerName = account?.AccNm ?? "Customer";
                            var ordMas = await db.OrdMas.FirstOrDefaultAsync(x => x.OrdKy == order.OrdKy.Value);
                            var invoiceNo = ordMas?.DocNo ?? order.ServiceOrdNo;
                            var totalAmount = await db.ServiceOrderDetail
                                .Where(d => d.ServiceOrdKy == order.ServiceOrdKy && d.IsApproved)
                                .SumAsync(d => (decimal?)d.Price) ?? 0;
                            var dateTime = AppTime.Now.ToString("yyyy-MM-dd HH:mm");
                            var sms = $"Dear {customerName},\n\n" +
                                      $"Your service has been successfully completed.\n\n" +
                                      $"Service Order No: {order.ServiceOrdNo}\n" +
                                      $"Date & Time: {dateTime}\n" +
                                      $"Invoice No: {invoiceNo}\n" +
                                      $"Total Amount: Rs. {totalAmount:N2}\n\n" +
                                      $"We appreciate your trust in our service. If you have any questions or need further assistance, feel free to contact us.\n\n" +
                                      $"Thank you again for your business.\n" +
                                      $"Best regards,\n" +
                                      $"HATCS";
                            _ = _smsService.SendAsync(adr.TP1, sms);
                        }
                    }
                }

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
                var p = o.PackageKy.HasValue ? await db.CdMas.FindAsync((short)o.PackageKy.Value) : null;
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
                var p = o.PackageKy.HasValue ? await db.CdMas.FindAsync((short)o.PackageKy.Value) : null;
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
            var p = o.PackageKy.HasValue ? await db.CdMas.FindAsync((short)o.PackageKy.Value) : null;
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

        private async Task<int> CreateOrdMasAndDetSync(DynamicDbContext db, ServiceOrder so, int userKey)
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
                nextOrdNo = 1;
                ordNoLst = new OrdNoLst
                {
                    CKy = (short)so.CKy,
                    OurCd = "SLSORD",
                    LstOrdNo = nextOrdNo,
                    fInAct = false,
                    Status = "A",
                    SKy = 1,
                    CdKy = ordTypKy
                };
                db.OrdNoLst.Add(ordNoLst);
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

            return ordMas.OrdKy;
        }

        private async Task CreateOrdDetSync(DynamicDbContext db, ServiceOrderDetail item)
        {
            var so = await db.ServiceOrder.FindAsync(item.ServiceOrdKy);
            if (so == null) return;

            int ordKy = so.OrdKy ?? 0;
            if (ordKy == 0)
            {
                var ordMas = await db.OrdMas.FirstOrDefaultAsync(x => x.DocNo == so.ServiceOrdNo && x.OrdTyp == "SLSORD");
                if (ordMas != null) ordKy = ordMas.OrdKy;
            }

            if (ordKy > 0)
            {
                await CreateOrdDetLogic(db, item, ordKy, item.EntUsrKy, null);
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

        private async Task SyncToTrnMas(DynamicDbContext db, int ordKy, int userKey)
        {
            var ordMas = await db.OrdMas.FirstOrDefaultAsync(x => x.OrdKy == ordKy);
            if (ordMas == null) return;

            // 1. Resolve TrnTypKy ("SALE")
            var trnTypCd = await db.CdMas.FirstOrDefaultAsync(c => c.OurCd == "SALE" && c.ConCd == "TrnTyp");
            short trnTypKy = (short)(trnTypCd?.CdKy ?? 1);

            // 2. Get Transaction Number
            var trnNoLst = await db.TrnNoLst.FirstOrDefaultAsync(x => x.OurCd == "SALE");
            int nextTrnNo = trnNoLst?.LstTrnNo ?? 0;

            // 3. Get Order Details
            var ordDets = await db.OrdDet.Where(x => x.Ordky == ordKy).ToListAsync();

            foreach (var det in ordDets)
            {
                nextTrnNo++;

                var trn = new TrnMas
                {
                    CKy = ordMas.CKy,
                    TrnDt = AppTime.Now,
                    TrnTypKy = trnTypKy,
                    TrnNo = nextTrnNo,
                    fInAct = false,
                    fApr = 1,
                    SKy = 1,
                    STrnKy = 1,
                    TrnTrfLnkKy = 1,
                    RepAdrKy = 1,
                    FyKy = 1,
                    OurCd = "SALE",
                    DocNo = ordMas.DocNo,
                    OrdKy = ordKy,
                    OrdDetKy = det.OrdDetKy,
                    YurRef = ordMas.YurRef,
                    YurDt = ordMas.OrdDt,
                    AdrKy = ordMas.Adrky,
                    AdrDetKy = 1,
                    ContraAccKy = 1,
                    AccKy = ordMas.AccKy ?? 1,
                    PmtTrmKy = ordMas.PmtTrmKy,
                    PmtModeKy = 1,
                    Amt = det.SlsPri ?? 0,
                    DisAmt = det.DisAmt,
                    DisPer = (float)det.DisPer,
                    BUKy = ordMas.BUKy,
                    LocKy = ordMas.LocKy,
                    ShiftKy = 1,
                    fTax = false,
                    fQtyPstd = true,
                    fValPstd = true,
                    fItmTrnVal = true,
                    fPrint = false,
                    TMf1 = false,
                    TMf2 = false,
                    tItmTrnRel = 1,
                    Des = det.Des,
                    Amt1 = det.Amt1,
                    Amt2 = det.Amt2,
                    CostCntrKy = 1,
                    PrntKy = 1,
                    Status = "A",
                    EntUsrKy = userKey,
                    EntDtm = AppTime.Now
                };

                db.TrnMas.Add(trn);
            }

            if (trnNoLst == null)
            {
                trnNoLst = new TrnNoLst
                {
                    OurCd = "SALE",
                    LstTrnNo = nextTrnNo,
                    fInAct = false,
                    CKy = ordMas.CKy,
                    SKy = 1,
                    CdKy = trnTypKy,
                    Status = "A"
                };
                db.TrnNoLst.Add(trnNoLst);
            }
            else
            {
                trnNoLst.LstTrnNo = nextTrnNo;
            }

            // Mark OrdMas as Finished
            ordMas.fFinish = true;

            await db.SaveChangesAsync();
        }

        public async Task<List<ServiceOrderImageDto>> GetOrderImagesAsync(int ordKy)
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.OrdImg
                .Where(x => x.OrdKy == ordKy && !x.fInAct)
                .Select(x => new ServiceOrderImageDto
                {
                    ImageKy = x.ImageKy,
                    ImageUrl = x.ImageUrl,
                    ImageType = x.ImageType
                })
                .ToListAsync();
        }
    }
}

