using Application.DTOs.Vehicle;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class VehicleService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public VehicleService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
        }

        // -------------------------------------------------------
        // REGISTER (ADD)
        // --------------------------------------------------------
        public async Task<(bool success, string message, int statusCode)> RegisterVehicleAsync(CreateVehicleRequestDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();

            var result = (success: false, message: string.Empty, statusCode: 500);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                    if (userKey == null)
                    {
                        userKey = 1;
                    }

                    // 1. Check if vehicle exists (Active only)
                    if (await db.Vehicles.AnyAsync(v => v.VehicleId == dto.VehicleId && v.fInAct != true))
                    {
                        result = (false, "Vehicle Number already exists", 409);
                        return;
                    }

                    // 2. Handle Owner — check/create Address
                    int adrKy;
                    if (dto.Owner.AdrKy.HasValue && dto.Owner.AdrKy > 0)
                    {
                        adrKy = dto.Owner.AdrKy.Value;
                    }
                    else
                    {
                        string expectedAdrNm = $"{dto.Owner.FstNm} {dto.Owner.LstNm}".Trim();
                        var existingAddress = await db.Addresses
                            .FirstOrDefaultAsync(a => a.CKy == (short)_userContext.CompanyKey && a.AdrNm == expectedAdrNm);

                        if (existingAddress != null)
                        {
                            adrKy = existingAddress.AdrKy;
                        }
                        else
                        {
                            var address = new AdrMas
                            {
                                CKy      = (short)_userContext.CompanyKey,
                                FstNm    = dto.Owner.FstNm,
                                LstNm    = dto.Owner.LstNm,
                                AdrNm    = expectedAdrNm,
                                Address  = dto.Owner.Address,
                                TP1      = dto.Owner.TP1,
                                EntUsrKy = userKey.Value,
                                EntDtm   = AppTime.Now
                            };
                            db.Addresses.Add(address);
                            await db.SaveChangesAsync();
                            adrKy = address.AdrKy;
                        }
                    }

                    // Check or create Account
                    var existingAccAdr = await db.AccAdr.FirstOrDefaultAsync(x => x.AdrKy == adrKy);
                    int accKy;

                    if (existingAccAdr != null)
                    {
                        accKy = existingAccAdr.AccKy;
                    }
                    else
                    {
                        var account = new Account
                        {
                            CKy       = (short)_userContext.CompanyKey,
                            AccCd     = "CUS" + adrKy,
                            AccNm     = $"{dto.Owner.FstNm} {dto.Owner.LstNm}",
                            AccTypKy  = 1,
                            AccTyp    = "CUS",
                            fInAct    = false,
                            fApr      = 1,
                            EntUsrKy  = userKey.Value,
                            EntDtm    = AppTime.Now,
                            SKy       = 1,
                            AccLvl    = 1,
                            fCusSup   = 1,
                            fCtrlAcc  = false,
                            fBasAcc   = true,
                            fMultiAdr = false,
                            CrLmt     = 0,
                            CrDays    = 0
                        };
                        db.Account.Add(account);
                        await db.SaveChangesAsync();
                        accKy = account.AccKy;

                        var accAdr = new AccAdr
                        {
                            AccKy = accKy,
                            AdrKy = adrKy,
                            A     = ""
                        };
                        db.AccAdr.Add(accAdr);
                        await db.SaveChangesAsync();
                    }

                    // 3. Create Vehicle
                    var vehicle = new Vehicle
                    {
                        VehicleId        = dto.VehicleId,
                        OwnerAccountKy   = accKy,
                        VehicleTypKy     = dto.VehicleTypKy,
                        FuelTyp          = dto.FuelTyp,
                        CurrentMileage   = dto.CurrentMileage,
                        MileageUpdateDtm = dto.CurrentMileage.HasValue ? AppTime.Now : null,
                        FuelLevel        = dto.FuelLevel,
                        Make             = dto.Make,
                        Model            = dto.Model,
                        Year             = dto.Year,
                        ChassisNo        = dto.ChassisNo,
                        EngineNo         = dto.EngineNo,
                        Description      = dto.Description,
                        fInAct           = false,
                        EntUsrKy         = userKey.Value,
                        EntDtm           = AppTime.Now
                    };
                    db.Vehicles.Add(vehicle);
                    await db.SaveChangesAsync(); // generates VehicleKy

                    // 4. Handle Drivers
                    foreach (var dDto in dto.Drivers)
                    {
                        int driverKy;
                        if (dDto.DriverKy.HasValue && dDto.DriverKy > 0)
                        {
                            driverKy = dDto.DriverKy.Value;
                        }
                        else
                        {
                            var newDriver = new Driver
                            {
                                DriverName = dDto.DriverName,
                                NIC        = dDto.NIC,
                                TP         = dDto.TP,
                                LicenseNo  = dDto.LicenseNo,
                                fInAct     = false,
                                EntUsrKy   = userKey.Value,
                                EntDtm     = AppTime.Now
                            };
                            db.Drivers.Add(newDriver);
                            await db.SaveChangesAsync();
                            driverKy = newDriver.DriverKy;
                        }

                        db.VehicleDrivers.Add(new VehicleDriver
                        {
                            VehicleKy = vehicle.VehicleKy,
                            DriverKy  = driverKy
                        });
                    }

                    await db.SaveChangesAsync();

                    // 5. Sync CusItm — create a matching item record in the same transaction
                    var cusItm = BuildCusItm(vehicle, _userContext.CompanyKey, userKey.Value);
                    db.CusItm.Add(cusItm);
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();
                    result = (true, "Vehicle registered successfully", 201);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var messages = new List<string>();
                    var current  = ex;
                    while (current != null)
                    {
                        messages.Add(current.Message);
                        current = current.InnerException;
                    }

                    result = (false, $"Error: {string.Join(" | ", messages)}", 500);
                }
            });

            return result;
        }

        // -------------------------------------------------------
        // UPDATE
        // -------------------------------------------------------
        public async Task<(bool success, string message)> UpdateVehicleAsync(CreateVehicleRequestDto dto)
        {
            if (!dto.VehicleKy.HasValue) return (false, "Vehicle Key is required");

            using var db = await _factory.CreateDbContextAsync();

            var result = (success: false, message: string.Empty);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                    if (userKey == null)
                    {
                        userKey = 1;
                    }

                    // --- Update Vehicle ---
                    var vehicle = await db.Vehicles.FindAsync(dto.VehicleKy.Value);
                    if (vehicle == null)
                    {
                        result = (false, "Vehicle not found");
                        return;
                    }

                    // Keep old ItmCd before changing VehicleId, so we can locate the CusItm row
                    string oldVehicleId = vehicle.VehicleId;

                    vehicle.VehicleId    = dto.VehicleId;
                    vehicle.VehicleTypKy = dto.VehicleTypKy;
                    vehicle.FuelTyp      = dto.FuelTyp;

                    if (dto.CurrentMileage != vehicle.CurrentMileage)
                    {
                        vehicle.CurrentMileage   = dto.CurrentMileage;
                        vehicle.MileageUpdateDtm = AppTime.Now;
                    }

                    vehicle.FuelLevel   = dto.FuelLevel;
                    vehicle.Make        = dto.Make;
                    vehicle.Model       = dto.Model;
                    vehicle.Year        = dto.Year;
                    vehicle.ChassisNo   = dto.ChassisNo;
                    vehicle.EngineNo    = dto.EngineNo;
                    vehicle.Description = dto.Description;

                    await db.SaveChangesAsync();

                    // --- Sync CusItm ---
                    string oldItmCd = oldVehicleId.Length > 15 ? oldVehicleId[..15] : oldVehicleId;

                    var cusItm = await db.CusItm
                        .FirstOrDefaultAsync(c => c.ItmCd == oldItmCd && c.CKy == _userContext.CompanyKey);

                    if (cusItm != null)
                    {
                        ApplyCusItmChanges(cusItm, vehicle, _userContext.CompanyKey);
                    }
                    else
                    {
                        // CusItm row missing — create it (self-healing)
                        db.CusItm.Add(BuildCusItm(vehicle, _userContext.CompanyKey, userKey.Value));
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    result = (true, "Vehicle updated successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result = (false, "Error updating vehicle: " + ex.Message);
                }
            });

            return result;
        }

        // -------------------------------------------------------
        // DELETE (soft-delete)
        // -------------------------------------------------------
        public async Task<(bool success, string message)> DeleteVehicleAsync(int vehicleKy)
        {
            using var db = await _factory.CreateDbContextAsync();

            var result = (success: false, message: string.Empty);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    var vehicle = await db.Vehicles.FindAsync(vehicleKy);
                    if (vehicle == null)
                    {
                        result = (false, "Vehicle not found");
                        return;
                    }

                    // Soft-delete vehicle
                    vehicle.fInAct = true;

                    // Soft-delete owner account if no other active vehicles remain
                    int? ownerAccKy = vehicle.OwnerAccountKy;
                    if (ownerAccKy.HasValue)
                    {
                        bool hasOtherVehicles = await db.Vehicles.AnyAsync(v =>
                            v.OwnerAccountKy == ownerAccKy.Value &&
                            v.VehicleKy != vehicleKy &&
                            v.fInAct != true);

                        if (!hasOtherVehicles)
                        {
                            var account = await db.Account.FindAsync(ownerAccKy.Value);
                            if (account != null) account.fInAct = true;
                        }
                    }

                    await db.SaveChangesAsync();

                    // --- Sync CusItm: soft-delete matching record ---
                    string itmCd = vehicle.VehicleId.Length > 15 ? vehicle.VehicleId[..15] : vehicle.VehicleId;

                    var cusItm = await db.CusItm
                        .FirstOrDefaultAsync(c => c.ItmCd == itmCd && c.CKy == _userContext.CompanyKey);

                    if (cusItm != null)
                    {
                        cusItm.fInAct = true;
                        cusItm.fObs   = true;  // mark as obsolete
                        await db.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    result = (true, "Vehicle deleted successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result = (false, ex.Message);
                }
            });

            return result;
        }

        // -------------------------------------------------------
        // READ — Active vehicle list
        // -------------------------------------------------------
        public async Task<List<VehicleListItemDto>> GetActiveVehiclesAsync()
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();
                return await db.Vehicles
                    .Where(v => v.fInAct != true)
                    .Select(v => new VehicleListItemDto
                    {
                        VehicleKy = v.VehicleKy,
                        VehicleId = v.VehicleId
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active vehicles: " + ex.Message);
            }
        }

        // -------------------------------------------------------
        // READ — All vehicles (detailed)
        // -------------------------------------------------------
        public async Task<List<VehicleDetailDto>> GetAllVehiclesDetailedAsync()
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();

                var vehicles = await db.Vehicles.AsNoTracking()
                    .Where(v => v.fInAct != true)
                    .ToListAsync();

                if (!vehicles.Any()) return new List<VehicleDetailDto>();

                var result = new List<VehicleDetailDto>();

                foreach (var vehicle in vehicles)
                {
                    Account? ownerAcc = null;
                    AdrMas?  adrMas   = null;

                    if (vehicle.OwnerAccountKy.HasValue)
                    {
                        ownerAcc = await db.Account.AsNoTracking()
                            .FirstOrDefaultAsync(a => a.AccKy == vehicle.OwnerAccountKy.Value);
                        var accAdr = await db.AccAdr
                            .FirstOrDefaultAsync(aa => aa.AccKy == vehicle.OwnerAccountKy.Value);
                        if (accAdr != null)
                            adrMas = await db.Addresses.AsNoTracking()
                                .FirstOrDefaultAsync(a => a.AdrKy == accAdr.AdrKy);
                    }

                    var drivers = await (from vd in db.VehicleDrivers
                                         join d in db.Drivers on vd.DriverKy equals d.DriverKy
                                         where vd.VehicleKy == vehicle.VehicleKy
                                         select new DriverDto
                                         {
                                             DriverKy   = d.DriverKy,
                                             DriverName = d.DriverName,
                                             NIC        = d.NIC,
                                             TP         = d.TP,
                                             LicenseNo  = d.LicenseNo
                                         }).ToListAsync();

                    string? vehicleType = "";
                    if (vehicle.VehicleTypKy.HasValue)
                    {
                        vehicleType = await db.CdMas
                            .Where(x => x.CdKy == vehicle.VehicleTypKy.Value
                                     && x.ConCd == "OrdCat1"
                                     && x.CKy   == _userContext.CompanyKey)
                            .Select(x => x.CdNm)
                            .FirstOrDefaultAsync();
                    }

                    result.Add(new VehicleDetailDto
                    {
                        VehicleKy        = vehicle.VehicleKy,
                        VehicleId        = vehicle.VehicleId,
                        VehicleTypKy     = vehicle.VehicleTypKy ?? 0,
                        VehicleTyp       = vehicleType ?? "",
                        FuelTyp          = vehicle.FuelTyp,
                        CurrentMileage   = vehicle.CurrentMileage,
                        MileageUpdateDtm = vehicle.MileageUpdateDtm,
                        FuelLevel        = vehicle.FuelLevel,
                        Make             = vehicle.Make,
                        Model            = vehicle.Model,
                        Year             = vehicle.Year,
                        ChassisNo        = vehicle.ChassisNo,
                        EngineNo         = vehicle.EngineNo,
                        Description      = vehicle.Description,
                        Owner = new OwnerDto
                        {
                            FstNm   = adrMas?.FstNm ?? ownerAcc?.AccNm ?? "",
                            LstNm   = adrMas?.LstNm ?? "",
                            Address = adrMas?.Address,
                            TP1     = adrMas?.TP1,
                            NIC     = adrMas?.AdrID1,
                        },
                        Drivers = drivers
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all vehicles detailed: " + ex.Message);
            }
        }

        // -------------------------------------------------------
        // READ — Single vehicle detail
        // -------------------------------------------------------
        public async Task<VehicleDetailDto?> GetVehicleDetailsAsync(int vehicleKy)
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();

                var vehicle = await db.Vehicles.AsNoTracking()
                    .FirstOrDefaultAsync(v => v.VehicleKy == vehicleKy);
                if (vehicle == null) return null;

                return await BuildVehicleDetailAsync(db, vehicle);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving vehicle details: " + ex.Message);
            }
        }

        public async Task<VehicleDetailDto?> GetVehicleDetailsByVehicleIdAsync(string vehicleId)
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();

                var vehicle = await db.Vehicles.AsNoTracking()
                    .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);
                if (vehicle == null) return null;

                return await BuildVehicleDetailAsync(db, vehicle);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving vehicle details by ID: " + ex.Message);
            }
        }

        private async Task<VehicleDetailDto> BuildVehicleDetailAsync(DynamicDbContext db, Vehicle vehicle)
        {
            Account? ownerAcc = null;
            AdrMas? adrMas = null;

            if (vehicle.OwnerAccountKy.HasValue)
            {
                ownerAcc = await db.Account.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AccKy == vehicle.OwnerAccountKy.Value);
                var accAdr = await db.AccAdr
                    .FirstOrDefaultAsync(aa => aa.AccKy == vehicle.OwnerAccountKy.Value);
                if (accAdr != null)
                    adrMas = await db.Addresses.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.AdrKy == accAdr.AdrKy);
            }

            var drivers = await (from vd in db.VehicleDrivers
                                 join d in db.Drivers on vd.DriverKy equals d.DriverKy
                                 where vd.VehicleKy == vehicle.VehicleKy
                                 select new DriverDto
                                 {
                                     DriverKy = d.DriverKy,
                                     DriverName = d.DriverName,
                                     NIC = d.NIC,
                                     TP = d.TP,
                                     LicenseNo = d.LicenseNo
                                 }).ToListAsync();

            string? vehicleType = "";
            if (vehicle.VehicleTypKy.HasValue)
            {
                vehicleType = await db.CdMas
                    .Where(x => x.CdKy == vehicle.VehicleTypKy.Value
                             && x.ConCd == "OrdCat1"
                             && x.CKy == _userContext.CompanyKey)
                    .Select(x => x.CdNm)
                    .FirstOrDefaultAsync();
            }

            return new VehicleDetailDto
            {
                VehicleKy = vehicle.VehicleKy,
                VehicleId = vehicle.VehicleId,
                VehicleTypKy = vehicle.VehicleTypKy ?? 0,
                VehicleTyp = vehicleType ?? "",
                FuelTyp = vehicle.FuelTyp,
                CurrentMileage = vehicle.CurrentMileage,
                MileageUpdateDtm = vehicle.MileageUpdateDtm,
                FuelLevel = vehicle.FuelLevel,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                ChassisNo = vehicle.ChassisNo,
                EngineNo = vehicle.EngineNo,
                Description = vehicle.Description,
                Owner = new OwnerDto
                {
                    FstNm = adrMas?.FstNm ?? ownerAcc?.AccNm ?? "",
                    LstNm = adrMas?.LstNm ?? "",
                    Address = adrMas?.Address,
                    TP1 = adrMas?.TP1,
                    NIC = adrMas?.AdrID1,
                },
                Drivers = drivers
            };
        }

        // -------------------------------------------------------
        // PRIVATE HELPERS — CusItm field mapping
        // -------------------------------------------------------

        /// <summary>
        /// Creates a new CusItm entity from a Vehicle.
        /// Call AFTER the vehicle has been saved (so VehicleKy is set).
        /// </summary>
        private static CusItm BuildCusItm(Vehicle vehicle, int companyKey, int userKey)
        {
            // ItmCd max 15 chars
            string itmCd = vehicle.VehicleId?.Length > 15
                ? vehicle.VehicleId[..15]
                : (vehicle.VehicleId ?? string.Empty);

            // Des max 60 chars
            string? des = vehicle.Description?.Length > 60
                ? vehicle.Description[..60]
                : vehicle.Description;

            // PartNo max 30 chars — use ChassisNo as the part reference
            string partNo = (vehicle.ChassisNo?.Length > 30
                ? vehicle.ChassisNo[..30]
                : vehicle.ChassisNo) ?? string.Empty;

            return new CusItm
            {
                CKy         = companyKey,
                ItmCd       = itmCd,
                fInAct      = vehicle.fInAct ?? false,
                fApr        = false,
                fObs        = false,
                CusItmTypKy = (short)(0),
                CusItmTyp   = "CUSITM",
                PartNo      = partNo,
                ItmNm       = vehicle.VehicleId,   // display name = plate number
                Des         = des,
                Make        = vehicle.Make,
                Model       = vehicle.Model,
                Wrnty       = 0,
                fCtrlItm    = false,
                fSrlNo      = true,                // vehicles are serial-number tracked
                UnitKy      = 0,
                CosPri      = 0,
                SlsPri      = 0,
                SlsPri2     = 0,
                EntUsrKy    = userKey,
                EntDtm      = AppTime.Now
            };
        }

        /// <summary>
        /// Applies updated vehicle values to an existing CusItm record.
        /// Audit fields (EntUsrKy / EntDtm) are preserved to maintain history.
        /// </summary>
        private static void ApplyCusItmChanges(CusItm cusItm, Vehicle vehicle, int companyKey)
        {
            string itmCd = vehicle.VehicleId?.Length > 15
                ? vehicle.VehicleId[..15]
                : (vehicle.VehicleId ?? string.Empty);

            string? des = vehicle.Description?.Length > 60
                ? vehicle.Description[..60]
                : vehicle.Description;

            string partNo = (vehicle.ChassisNo?.Length > 30
                ? vehicle.ChassisNo[..30]
                : vehicle.ChassisNo) ?? string.Empty;

            cusItm.CKy         = companyKey;
            cusItm.ItmCd       = itmCd;
            cusItm.ItmNm       = vehicle.VehicleId;
            cusItm.Des         = des;
            cusItm.PartNo      = partNo;
            cusItm.Make        = vehicle.Make;
            cusItm.Model       = vehicle.Model;
            cusItm.CusItmTypKy = (short)(vehicle.VehicleTypKy ?? 0);
            cusItm.fInAct      = vehicle.fInAct ?? false;
            // NOTE: EntUsrKy and EntDtm intentionally not updated — preserve original audit trail
        }
    }
}

