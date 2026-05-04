using Application.DTOs.Reservation;
using Application.DTOs.Vehicle;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class ReservationService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;
        private readonly VehicleService _vehicleService;
        private readonly BayControlService _bayControlService;
        private readonly ISmsService _smsService;

        public ReservationService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService,
            VehicleService vehicleService,
            BayControlService bayControlService,
            ISmsService smsService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
            _vehicleService = vehicleService;
            _bayControlService = bayControlService;
            _smsService = smsService;
        }

        public async Task<(bool success, string message, int resKy)> CreateReservationAsync(CreateFullReservationDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();

            var result = (success: false, message: string.Empty, resKy: 0);

            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    // 1. Handle Vehicle (Get Existing or Register New)
                    int vehicleKy = 0;
                    var existingVehicle = await db.Vehicles
                        .FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId && v.fInAct != true);

                    if (existingVehicle != null)
                    {
                        vehicleKy = existingVehicle.VehicleKy;
                    }
                    else
                    {
                        if (dto.NewVehicleDetails == null)
                        {
                            result = (false, "Vehicle not found. Please provide vehicle registration details.", 0);
                            return;
                        }

                        // Ensure ID matches
                        dto.NewVehicleDetails.VehicleId = dto.VehicleId;

                        // Call VehicleService to register
                        var regResult = await _vehicleService.RegisterVehicleAsync(dto.NewVehicleDetails);
                        if (!regResult.success)
                        {
                            result = (false, $"Vehicle Registration Failed: {regResult.message}", 0);
                            return;
                        }

                        // Fetch the newly created vehicle key
                        var newVeh = await db.Vehicles
                            .FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId);

                        if (newVeh == null) throw new Exception("Failed to retrieve new vehicle after registration.");
                        vehicleKy = newVeh.VehicleKy;
                    }

                    var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, 1);

                    // 2. Create ReservationMas
                    var resMas = new ReservationMas
                    {
                        VehicleKy = vehicleKy,
                        PackageKy = dto.PackageKy,
                        ResStatus = "Pending",
                        Remarks = dto.Remarks,
                        Tp1 = dto.Tp1,
                        fInAct = false,
                        EntUsrKy = userKey ?? 0,
                        EntDtm = AppTime.Now,
                        CKy = _userContext.CompanyKey
                    };

                    db.ReservationMas.Add(resMas);
                    await db.SaveChangesAsync(); // Get ResKy

                    // 3. Check if Date is Unavailable (Holiday/Event)
                    var bookingDate = dto.BookingFrom.Date;
                    var unavailableDate = await db.CalendarMas
                        .FirstOrDefaultAsync(c => c.CalDt != null && c.CalDt.Value.Date == bookingDate && !c.fInAct);

                    if (unavailableDate != null)
                    {
                        await transaction.RollbackAsync();
                        result = (false, $"Selected date is unavailable: {unavailableDate.CalDesc}", 0);
                        return;
                    }

                    // 4. Overlap Check
                    int overlapCount = await db.BayReservations.CountAsync(r =>
                        !r.fInAct && r.ResStatus != "Cancelled" &&
                        ((dto.BookingFrom >= r.FromDtm && dto.BookingFrom < r.ToDtm) ||
                         (dto.BookingTo > r.FromDtm && dto.BookingTo <= r.ToDtm) ||
                         (dto.BookingFrom <= r.FromDtm && dto.BookingTo >= r.ToDtm)));

                    if (overlapCount >= 3)
                    {
                        await transaction.RollbackAsync();
                        result = (false, "Validation Failed: Selected time period already has the maximum number of reservations (3).", 0);
                        return;
                    }

                    var bayRes = new BayReservation
                    {
                        BayKy = dto.BayKy,
                        ReservationMasKy = resMas.ResKy,
                        VehicleKy = vehicleKy,
                        FromDtm = dto.BookingFrom,
                        ToDtm = dto.BookingTo,
                        ResType = "Online",
                        ResStatus = "Pending",
                        fInAct = false,
                        EntUsrKy = userKey ?? 0,
                        EntDtm = AppTime.Now,
                        CKy = _userContext.CompanyKey
                    };

                    db.BayReservations.Add(bayRes);
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();
                    result = (true, "Reservation placed successfully, awaiting approval.", resMas.ResKy);

                    // SMS notification after reservation creation has been disabled as per request

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result = (false, $"Error: {ex.Message}", 0);
                }
            });

            return result;
        }

        public async Task<(bool success, string message)> UpdateReservationAsync(int resKy, CreateFullReservationDto dto)
        {
             using var db = await _factory.CreateDbContextAsync();
             try
             {
                 var res = await db.ReservationMas.FindAsync(resKy);
                 if (res == null || res.fInAct) return (false, "Reservation not found");

                 // Update simple fields
                 res.PackageKy = dto.PackageKy;
                 res.Remarks = dto.Remarks;
                 // Vehicle update? complicated. Assume vehicle doesn't change for a reservation usually.
                 
                 // Update Bay Reservation Slot?
                 var bayRes = await db.BayReservations.FirstOrDefaultAsync(r => r.ReservationMasKy == resKy && !r.fInAct);
                 if (bayRes != null)
                 {
                     // Check if times changed, validate overlap again
                     if (bayRes.FromDtm != dto.BookingFrom || bayRes.ToDtm != dto.BookingTo || bayRes.BayKy != dto.BayKy)
                     {
                        int overlapCount = await db.BayReservations.CountAsync(r => 
                            r.ReservationMasKy != resKy && // Exclude self
                            !r.fInAct && r.ResStatus != "Cancelled" &&
                            ((dto.BookingFrom >= r.FromDtm && dto.BookingFrom < r.ToDtm) ||
                             (dto.BookingTo > r.FromDtm && dto.BookingTo <= r.ToDtm) ||
                             (dto.BookingFrom <= r.FromDtm && dto.BookingTo >= r.ToDtm)));
                        
                        if (overlapCount >= 3) return (false, "Validation Failed: Selected time period already has the maximum number of reservations (3).");

                        bayRes.BayKy = dto.BayKy;
                        bayRes.FromDtm = dto.BookingFrom;
                        bayRes.ToDtm = dto.BookingTo;
                     }
                 }
                 
                 await db.SaveChangesAsync();
                 return (true, "Reservation updated");
             }
             catch (Exception ex)
             {
                 return (false, "Error updating reservation: " + ex.Message);
             }
        }

        public async Task<(bool success, string message)> DeleteReservationAsync(int resKy)
        {
             using var db = await _factory.CreateDbContextAsync();
             try
             {
                 var res = await db.ReservationMas.FindAsync(resKy);
                 if (res == null) return (false, "Not found");

                 res.fInAct = true;
                 
                 var bayRes = await db.BayReservations.FirstOrDefaultAsync(r => r.ReservationMasKy == resKy && !r.fInAct);
                 if (bayRes != null)
                 {
                     bayRes.fInAct = true;
                     bayRes.ResStatus = "Cancelled";
                 }

                 await db.SaveChangesAsync();
                 return (true, "Reservation deleted");
             }
             catch (Exception ex)
             {
                 return (false, "Error deleting reservation: " + ex.Message);
             }
        }

        public async Task<(bool success, string message)> ApproveReservationAsync(int resKy, bool approve)
        {
             using var db = await _factory.CreateDbContextAsync();
             try
             {
                 var res = await db.ReservationMas.FindAsync(resKy);
                 if (res == null) return (false, "Not found");
                 
                 string status = approve ? "Approved" : "Cancelled"; // Or Rejected

                 res.ResStatus = status;

                 var bayRes = await db.BayReservations.FirstOrDefaultAsync(r => r.ReservationMasKy == resKy && !r.fInAct);
                 if (bayRes != null)
                 {
                     bayRes.ResStatus = status;
                 }

                 await db.SaveChangesAsync();
                 return (true, $"Reservation {status}");
             }
             catch (Exception ex)
             {
                 return (false, "Error processing approval: " + ex.Message);
             }
        }

        public async Task<List<ReservationDetailDto>> GetReservationsAsync(string? vehicleId, DateTime? date)
        {
            using var db = await _factory.CreateDbContextAsync();
            
            var query = from r in db.ReservationMas
                        join br in db.BayReservations on r.ResKy equals br.ReservationMasKy into brGroup
                        from br in brGroup.DefaultIfEmpty()
                        join v in db.Vehicles on r.VehicleKy equals v.VehicleKy into vGroup
                        from v in vGroup.DefaultIfEmpty()
                        join p in db.CdMas on (short?)(r.PackageKy.HasValue ? r.PackageKy.Value : (int?)null) equals (short?)p.CdKy into pkg
                        from p in pkg.DefaultIfEmpty()
                        join b in db.Bays on (int?)(br == null ? null : br.BayKy) equals (int?)b.BayKy into bGroup
                        from b in bGroup.DefaultIfEmpty()
                        // Left join Account
                        join acc in db.Account on (int?)(v == null ? null : v.OwnerAccountKy) equals (int?)acc.AccKy into accGroup
                        from acc in accGroup.DefaultIfEmpty()
                        // Left join VehicleType
                        join vt in db.CdMas on (int?)(v == null ? null : v.VehicleTypKy) equals (int?)vt.CdKy into vtGroup
                        from vt in vtGroup.DefaultIfEmpty()
                        // Left join Address for Phone
                        join accAdr in db.AccAdr on (int?)(v == null ? null : v.OwnerAccountKy) equals (int?)accAdr.AccKy into accAdrGroup
                        from accAdr in accAdrGroup.DefaultIfEmpty()
                        join adr in db.Addresses on (int?)(accAdr == null ? null : accAdr.AdrKy) equals (int?)adr.AdrKy into adrGroup
                        from adr in adrGroup.DefaultIfEmpty()
                        where !r.fInAct
                        select new { r, br, v, p, b, acc, vt, adr };

            if (!string.IsNullOrEmpty(vehicleId))
                query = query.Where(x => x.v != null && x.v.VehicleId == vehicleId);

            if (date.HasValue)
                query = query.Where(x => x.br != null && x.br.FromDtm.Date == date.Value.Date);

            var result = await query.Select(x => new ReservationDetailDto
            {
                ResKy = x.r.ResKy,
                VehicleKy = x.r.VehicleKy,
                VehicleId = x.v != null ? x.v.VehicleId : "",
                VehicleType = x.vt != null ? x.vt.CdNm : "",
                OwnerName = x.acc != null ? x.acc.AccNm : "Unknown",
                OwnerPhone = x.adr != null && x.adr.TP1 != null ? x.adr.TP1 : "",
                PackageKy = x.r.PackageKy,
                PackageName = x.p != null ? x.p.CdNm : "Unknown",
                BayKy = x.br != null ? x.br.BayKy : 0,
                BayName = x.b != null ? x.b.BayNm : "",
                FromDtm = x.br != null ? x.br.FromDtm : default(DateTime),
                ToDtm = x.br != null ? x.br.ToDtm : default(DateTime),
                ResStatus = x.r.ResStatus,
                Remarks = x.r.Remarks
            }).ToListAsync();

            return result;
        }
    }
}

