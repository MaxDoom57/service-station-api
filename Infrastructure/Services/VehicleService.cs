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

        public async Task<(bool success, string message, int statusCode)> RegisterVehicleAsync(CreateVehicleRequestDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                if (userKey == null) return (false, "User key not found", 400);

                // 1. Check if vehicle exists (Active only)
                if (await db.Vehicles.AnyAsync(v => v.VehicleId == dto.VehicleId && v.fInAct != true))
                    return (false, "Vehicle Number already exists", 409);

                // 2. Handle Owner
                // Check or Create Address
                int adrKy;
                if (dto.Owner.AdrKy.HasValue && dto.Owner.AdrKy > 0)
                {
                    adrKy = dto.Owner.AdrKy.Value;
                }
                else
                {
                    // Create Address
                    var address = new AdrMas
                    {
                        CKy = (short)_userContext.CompanyKey,
                        FstNm = dto.Owner.FstNm,
                        LstNm = dto.Owner.LstNm,
                        //AccNm = $"{dto.Owner.FstNm} {dto.Owner.LstNm}", // Assuming AdrNm/AccNm logic
                        AdrNm = $"{dto.Owner.FstNm} {dto.Owner.LstNm}",
                        Address = dto.Owner.Address,
                        TP1 = dto.Owner.TP1,
                        //NIC = dto.Owner.NIC, // If AdrMas has NIC
                        EntUsrKy = userKey.Value,
                        EntDtm = DateTime.Now
                    };
                    db.Addresses.Add(address);
                    await db.SaveChangesAsync();
                    adrKy = address.AdrKy;
                }

                // Check or Create Account
                // Usually we search if an account exists for this address or create new
                // For simplicity, we create a new CUS account linking to this address
                // Ideally check if Acc already exists for this AdrKy
                var existingAccAdr = await db.AccAdr.FirstOrDefaultAsync(x => x.AdrKy == adrKy);
                int accKy;

                if (existingAccAdr != null)
                {
                    accKy = existingAccAdr.AccKy;
                }
                else
                {
                    // Create Account
                    var account = new Account
                    {
                        CKy = (short)_userContext.CompanyKey,
                        AccCd = "CUS" + adrKy, // Simple generation logic
                        AccNm = $"{dto.Owner.FstNm} {dto.Owner.LstNm}",
                        AccTypKy = 1, // Assuming 1 is Customer, need to use proper lookup
                        AccTyp = "CUS",
                        fInAct = false,
                        fApr = 1, // Approved
                        EntUsrKy = userKey.Value,
                        EntDtm = DateTime.Now,
                        SKy = 1, // Store Key default
                        AccLvl = 1, 
                        fCusSup = 1, // Is Customer/Supplier
                        fCtrlAcc = false,
                        fBasAcc = true,
                        fMultiAdr = false,
                        //fDefault = false, // Removed
                        //Defaults
                        CrLmt = 0,
                        CrDays = 0
                    };
                    db.Account.Add(account);
                    await db.SaveChangesAsync();
                    accKy = account.AccKy;

                    // Link Account and Address
                    var accAdr = new AccAdr
                    {
                        AccKy = accKy,
                        AdrKy = adrKy,
                         // A is unknown from context, assuming empty or default
                         A = ""
                    };
                    db.AccAdr.Add(accAdr);
                    await db.SaveChangesAsync();
                }

                // 3. Create Vehicle
                var vehicle = new Vehicle
                {
                    VehicleId = dto.VehicleId,
                    OwnerAccountKy = accKy,
                    VehicleTypKy = dto.VehicleTypKy,
                    FuelTyp = dto.FuelTyp,
                    CurrentMileage = dto.CurrentMileage,
                    MileageUpdateDtm = dto.CurrentMileage.HasValue ? DateTime.Now : null,
                    FuelLevel = dto.FuelLevel,
                    Make = dto.Make,
                    Model = dto.Model,
                    Year = dto.Year,
                    ChassisNo = dto.ChassisNo,
                    EngineNo = dto.EngineNo,
                    Description = dto.Description,
                    fInAct = false,
                    EntUsrKy = userKey.Value,
                    EntDtm = DateTime.Now
                };
                db.Vehicles.Add(vehicle);
                await db.SaveChangesAsync();

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
                            NIC = dDto.NIC,
                            TP = dDto.TP,
                            LicenseNo = dDto.LicenseNo,
                            fInAct = false,
                            EntUsrKy = userKey.Value,
                            EntDtm = DateTime.Now
                        };
                        db.Drivers.Add(newDriver);
                        await db.SaveChangesAsync();
                        driverKy = newDriver.DriverKy;
                    }

                    var vDriver = new VehicleDriver
                    {
                        VehicleKy = vehicle.VehicleKy,
                        DriverKy = driverKy
                    };
                    db.VehicleDrivers.Add(vDriver);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Vehicle registered successfully", 201);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                var messages = new List<string>();
                var current = ex;

                while (current != null)
                {
                    messages.Add(current.Message);
                    current = current.InnerException;
                }

                var detailedError = string.Join(" | ", messages);

                return (false, $"Error: {detailedError}", 500);
            }
        }

        public async Task<(bool success, string message)> UpdateVehicleAsync(CreateVehicleRequestDto dto)
        {
             if (!dto.VehicleKy.HasValue) return (false, "Vehicle Key is required");

            try
            {
                using var db = await _factory.CreateDbContextAsync();
                var vehicle = await db.Vehicles.FindAsync(dto.VehicleKy.Value);
            
                if (vehicle == null) return (false, "Vehicle not found");

                vehicle.VehicleId = dto.VehicleId;
                vehicle.VehicleTypKy = dto.VehicleTypKy;
                vehicle.FuelTyp = dto.FuelTyp;

                // Update Mileage Logic
                if (dto.CurrentMileage != vehicle.CurrentMileage)
                {
                     vehicle.CurrentMileage = dto.CurrentMileage;
                     vehicle.MileageUpdateDtm = DateTime.Now;
                }

                vehicle.FuelLevel = dto.FuelLevel;
                vehicle.Make = dto.Make;
                vehicle.Model = dto.Model;
                vehicle.Year = dto.Year;
                vehicle.ChassisNo = dto.ChassisNo;
                vehicle.EngineNo = dto.EngineNo;
                vehicle.Description = dto.Description;

                await db.SaveChangesAsync();
                return (true, "Vehicle updated successfully");
            }
            catch (Exception ex)
            {
                return (false, "Error updating vehicle: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeleteVehicleAsync(int vehicleKy)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var transaction = await db.Database.BeginTransactionAsync();

            try 
            {
                var vehicle = await db.Vehicles.FindAsync(vehicleKy);
                if (vehicle == null) return (false, "Vehicle not found");

                vehicle.fInAct = true;
                
                // Logic: Only delete customer account/address if NOT registered to other active vehicles
                int? ownerAccKy = vehicle.OwnerAccountKy;

                if (ownerAccKy.HasValue)
                {
                    bool hasOtherVehicles = await db.Vehicles.AnyAsync(v => v.OwnerAccountKy == ownerAccKy.Value && v.VehicleKy != vehicleKy && v.fInAct != true);

                    if (!hasOtherVehicles)
                    {
                        var account = await db.Account.FindAsync(ownerAccKy.Value);
                        if (account != null) account.fInAct = true;

                        // Find Linked Addresses
                        var accAdrs = await db.AccAdr.Where(x => x.AccKy == ownerAccKy.Value).ToListAsync();
                        // Optional: Invalidate addresses or leave them. Usually addresses are reusable or kept for history.
                        // Given previous errors, avoid touching unknown columns.
                    }
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Vehicle deleted successfully");
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message);
            }
        }

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

        public async Task<List<VehicleDetailDto>> GetAllVehiclesDetailedAsync()
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();
            
                // Fetch all active vehicles
                var vehicles = await db.Vehicles.AsNoTracking()
                    .Where(v => v.fInAct != true)
                    .ToListAsync();

                if (!vehicles.Any()) return new List<VehicleDetailDto>();

                var result = new List<VehicleDetailDto>();

                foreach (var vehicle in vehicles)
                {
                    // Get Owner Logic (Account -> AccAdr -> Address) - Optimized to be fetched per vehicle or batch loaded
                    // For simplicity and correctness with existing entities, we fetch per vehicle (N+1 but acceptable for small scale or can optimize later)
                
                    Account? ownerAcc = null;
                    // Wait, entity is AdrMas.
                    AdrMas? adrMas = null;

                    if (vehicle.OwnerAccountKy.HasValue)
                    {
                        ownerAcc = await db.Account.AsNoTracking().FirstOrDefaultAsync(a => a.AccKy == vehicle.OwnerAccountKy.Value);
                        var accAdr = await db.AccAdr.FirstOrDefaultAsync(aa => aa.AccKy == vehicle.OwnerAccountKy.Value);
                        if (accAdr != null) 
                        { 
                           adrMas = await db.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.AdrKy == accAdr.AdrKy);
                        }
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

                    // Get VehicleType Name
                    string? vehicleType = "";
                    if (vehicle.VehicleTypKy.HasValue)
                    {
                        vehicleType = await db.CdMas
                            .Where(x => x.CdKy == vehicle.VehicleTypKy.Value && x.ConCd == "OrdCat1" && x.CKy == _userContext.CompanyKey)
                            .Select(x => x.CdNm)
                            .FirstOrDefaultAsync();
                    }

                    result.Add(new VehicleDetailDto
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
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all vehicles detailed: " + ex.Message);
            }
        }

        public async Task<VehicleDetailDto?> GetVehicleDetailsAsync(int vehicleKy)
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();
            
                var vehicle = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.VehicleKy == vehicleKy);
                if (vehicle == null) return null;

                // Get Owner Logic (Account -> AccAdr -> Address)
                Account? ownerAcc = null;
                AdrMas? adrMas = null;

                if (vehicle.OwnerAccountKy.HasValue)
                {
                    ownerAcc = await db.Account.AsNoTracking().FirstOrDefaultAsync(a => a.AccKy == vehicle.OwnerAccountKy.Value);
                    var accAdr = await db.AccAdr.FirstOrDefaultAsync(aa => aa.AccKy == vehicle.OwnerAccountKy.Value);
                    if (accAdr != null) 
                    { 
                       adrMas = await db.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.AdrKy == accAdr.AdrKy);
                    }
                }

                // Get Code Master for VehicleType if needed? 
                // "use cdMas table for get ... vehicle type". 
                // We can join or just return ID. The requirement emphasizes returning details.

                var drivers = await (from vd in db.VehicleDrivers
                                     join d in db.Drivers on vd.DriverKy equals d.DriverKy
                                     where vd.VehicleKy == vehicleKy
                                     select new DriverDto
                                     {
                                         DriverKy = d.DriverKy,
                                         DriverName = d.DriverName,
                                         NIC = d.NIC,
                                         TP = d.TP,
                                         LicenseNo = d.LicenseNo
                                     }).ToListAsync();

                // Get VehicleType Name
                string? vehicleType = "";
                if (vehicle.VehicleTypKy.HasValue)
                {
                    vehicleType = await db.CdMas
                        .Where(x => x.CdKy == vehicle.VehicleTypKy.Value && x.ConCd == "OrdCat1" && x.CKy == _userContext.CompanyKey)
                        .Select(x => x.CdNm)
                        .FirstOrDefaultAsync();
                }

                return new VehicleDetailDto
                {
                    VehicleKy = vehicle.VehicleKy,
                    VehicleId = vehicle.VehicleId,
                    VehicleTypKy = vehicle.VehicleTypKy ?? 0,
                    VehicleTyp = vehicleType ?? "", // Populated from CdMas
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
                        LstNm = adrMas?.LstNm ?? "", // Account might not have LstNm split
                        Address = adrMas?.Address,
                        TP1 = adrMas?.TP1,
                        NIC = adrMas?.AdrID1, // Mapping AdrID1 to NIC as fallback
                    },
                    Drivers = drivers
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving vehicle details: " + ex.Message);
            }
        }
    }
}
