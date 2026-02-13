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

        public ReservationService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService,
            VehicleService vehicleService,
            BayControlService bayControlService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
            _vehicleService = vehicleService;
            _bayControlService = bayControlService;
        }

        public async Task<(bool success, string message, int resKy)> CreateReservationAsync(CreateFullReservationDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                // 1. Handle Vehicle (Get Existiing or Register New)
                int vehicleKy = 0;
                var existingVehicle = await db.Vehicles
                    .FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId && !v.fInAct);

                if (existingVehicle != null)
                {
                    vehicleKy = existingVehicle.VehicleKy;
                }
                else
                {
                    if (dto.NewVehicleDetails == null)
                        return (false, "Vehicle not found. Please provide vehicle registration details.", 0);

                    // Ensure ID matches
                    dto.NewVehicleDetails.VehicleId = dto.VehicleId;

                    // Call VehicleService to register
                    var regResult = await _vehicleService.RegisterVehicleAsync(dto.NewVehicleDetails);
                    if (!regResult.success) return (false, $"Vehicle Registration Failed: {regResult.message}", 0);
                    
                    // Fetch the newly created vehicle key
                    // VehicleService returns success message but not ID currently (based on previous code). 
                    // I will fetch it again.
                    var newVeh = await db.Vehicles
                        .FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId);
                    
                    if (newVeh == null) throw new Exception("Failed to retrieve new vehicle after registration.");
                    vehicleKy = newVeh.VehicleKy;
                }

                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);

                // 2. Create ReservationMas
                var resMas = new ReservationMas
                {
                    VehicleKy = vehicleKy,
                    PackageKy = dto.PackageKy,
                    ResStatus = "Pending",
                    Remarks = dto.Remarks,
                    fInAct = false,
                    EntUsrKy = userKey ?? 0,
                    EntDtm = DateTime.Now,
                    CKy = _userContext.CompanyKey
                };

                db.ReservationMas.Add(resMas);
                await db.SaveChangesAsync(); // Get ResKy

                // 3. Create BayReservation (The Slot)
                // Use BayControlService to validate overlap? 
                // Or I can insert directly since BayControlService logic for "Pending" allows insertion check.
                // But I should duplicate the logic or use internal helper to avoid circular DI or context issues if I used Service directly. Used Db here.

                // Check if Date is Unavailable (Holiday/Event)
                var bookingDate = dto.BookingFrom.Date;
                var unavailableDate = await db.CalendarMas
                    .FirstOrDefaultAsync(c => c.CalDt != null && c.CalDt.Value.Date == bookingDate && !c.fInAct); // Check if CalDt matches Booking Date

                if (unavailableDate != null)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Selected date is unavailable: {unavailableDate.CalDesc}", 0);
                }

                // Overlap Check
                bool overlap = await db.BayReservations.AnyAsync(r => 
                    r.BayKy == dto.BayKy && !r.fInAct && r.ResStatus != "Cancelled" &&
                    ((dto.BookingFrom >= r.FromDtm && dto.BookingFrom < r.ToDtm) ||
                     (dto.BookingTo > r.FromDtm && dto.BookingTo <= r.ToDtm) ||
                     (dto.BookingFrom <= r.FromDtm && dto.BookingTo >= r.ToDtm)));

                if (overlap) 
                {
                     await transaction.RollbackAsync();
                     return (false, "Validation Failed: Selected Bay Time Slot is already reserved/requested.", 0);
                }

                var bayRes = new BayReservation
                {
                    BayKy = dto.BayKy,
                    ReservationMasKy = resMas.ResKy, // Link to Parent
                    VehicleKy = vehicleKy,
                    FromDtm = dto.BookingFrom,
                    ToDtm = dto.BookingTo,
                    ResType = "Online", // Or 'Physical' passed in? Assuming this flow is for Booking.
                    ResStatus = "Pending",
                    fInAct = false,
                    EntUsrKy = userKey ?? 0,
                    EntDtm = DateTime.Now,
                    CKy = _userContext.CompanyKey
                };

                db.BayReservations.Add(bayRes);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, "Reservation placed successfully, awaiting approval.", resMas.ResKy);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> UpdateReservationAsync(int resKy, CreateFullReservationDto dto)
        {
             using var db = await _factory.CreateDbContextAsync();
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
                    bool overlap = await db.BayReservations.AnyAsync(r => 
                        r.ReservationMasKy != resKy && // Exclude self
                        r.BayKy == dto.BayKy && !r.fInAct && r.ResStatus != "Cancelled" &&
                        ((dto.BookingFrom >= r.FromDtm && dto.BookingFrom < r.ToDtm) ||
                         (dto.BookingTo > r.FromDtm && dto.BookingTo <= r.ToDtm) ||
                         (dto.BookingFrom <= r.FromDtm && dto.BookingTo >= r.ToDtm)));
                    
                    if (overlap) return (false, "New slot is unavailable");

                    bayRes.BayKy = dto.BayKy;
                    bayRes.FromDtm = dto.BookingFrom;
                    bayRes.ToDtm = dto.BookingTo;
                 }
             }
             
             await db.SaveChangesAsync();
             return (true, "Reservation updated");
        }

        public async Task<(bool success, string message)> DeleteReservationAsync(int resKy)
        {
             using var db = await _factory.CreateDbContextAsync();
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

        public async Task<(bool success, string message)> ApproveReservationAsync(int resKy, bool approve)
        {
             using var db = await _factory.CreateDbContextAsync();
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

        public async Task<List<ReservationDetailDto>> GetReservationsAsync(string? vehicleId, DateTime? date)
        {
            using var db = await _factory.CreateDbContextAsync();
            
            var query = from r in db.ReservationMas
                        join br in db.BayReservations on r.ResKy equals br.ReservationMasKy
                        join v in db.Vehicles on r.VehicleKy equals v.VehicleKy
                        join p in db.CdMas on r.PackageKy equals p.CdKy into pkg
                        from p in pkg.DefaultIfEmpty()
                        join b in db.Bays on br.BayKy equals b.BayKy
                        // Join Owner details
                        // Vehicle has OwnerAccountKy. Link 'Account' table? And 'AccAdr'?
                        // Lets do simple fetch first.
                        join acc in db.Account on v.OwnerAccountKy equals acc.AccKy
                        // join adr ... (VehicleService has logic to get Owner Name. I'll rely on Account Name)
                        where r.CKy == _userContext.CompanyKey && !r.fInAct
                        select new { r, br, v, p, b, acc };

            if (!string.IsNullOrEmpty(vehicleId))
                query = query.Where(x => x.v.VehicleId == vehicleId);

            if (date.HasValue)
                query = query.Where(x => x.br.FromDtm.Date == date.Value.Date);

            var result = await query.Select(x => new ReservationDetailDto
            {
                ResKy = x.r.ResKy,
                VehicleKy = x.r.VehicleKy,
                VehicleId = x.v.VehicleId,
                VehicleType = "", // Can fetch
                OwnerName = x.acc.AccNm,
                OwnerPhone = "", // Need Address link
                PackageKy = x.r.PackageKy,
                PackageName = x.p != null ? x.p.CdNm : "Unknown",
                BayKy = x.br.BayKy,
                BayName = x.b.BayNm,
                FromDtm = x.br.FromDtm,
                ToDtm = x.br.ToDtm,
                ResStatus = x.r.ResStatus,
                Remarks = x.r.Remarks
            }).ToListAsync();

            return result;
        }
    }
}
