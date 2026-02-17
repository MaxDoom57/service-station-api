using Application.DTOs.BayControl;
using Application.DTOs.Bay;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class BayControlService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public BayControlService(
            IDynamicDbContextFactory factory,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _factory = factory;
            _userContext = userContext;
            _userKeyService = userKeyService;
        }

        // 1. Get Available Bays (Now)
        // No vehicle at the bay AND No active reservation for NOW.
        // "Real" reservation = Approved.
        public async Task<List<AvailableBayDto>> GetAvailableBaysNowAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            var now = DateTime.Now;

            try
            {
                // Get All Active Bays
                var allBays = await db.Bays
                    .Where(b => !b.fInAct)
                    .ToListAsync();

                var availableBays = new List<AvailableBayDto>();

                foreach (var bay in allBays)
                {
                    // Check BayControl (Real-time)
                    var control = await db.BayControls
                        .FirstOrDefaultAsync(c => c.BayKy == bay.BayKy);
                    
                    bool isPhysicallyOccupied = control != null && control.IsBayOccupied;

                    // Check Reservations (Scheduled for NOW and Approved)
                    bool isReservedNow = await db.BayReservations.AnyAsync(r => 
                        r.BayKy == bay.BayKy && 
                        !r.fInAct && 
                        r.ResStatus == "Approved" && 
                        r.FromDtm <= now && r.ToDtm >= now
                    );

                    if (!isPhysicallyOccupied && !isReservedNow)
                    {
                        availableBays.Add(new AvailableBayDto
                        {
                            BayKy = bay.BayKy,
                            BayCd = bay.BayCd,
                            BayNm = bay.BayNm,
                            Status = "Available",
                            CurrentActivity = null
                        });
                    }
                }
                return availableBays;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving available bays: " + ex.Message);
            }
        }

        // 2. Get All Bays Status (With Vehicle ID logic)
        public async Task<List<BayStatusDto>> GetAllBaysStatusAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            try
            {
                var result = await (from b in db.Bays
                                    join c in db.BayControls on b.BayKy equals c.BayKy into bc
                                    from c in bc.DefaultIfEmpty()
                                    join v in db.Vehicles on c.CurrentVehicleKy equals v.VehicleKy into bv
                                    from v in bv.DefaultIfEmpty()
                                    where !b.fInAct
                                    select new BayStatusDto
                                    {
                                        BayKy = b.BayKy,
                                        BayCd = b.BayCd,
                                        BayNm = b.BayNm,
                                        IsOccupied = c != null && c.IsBayOccupied,
                                        CurrentVehicleKy = c != null ? c.CurrentVehicleKy : null,
                                        VehicleNumber = v != null ? v.VehicleId : null,
                                        CurrentActivity = c != null ? c.CurrentActivity : null,
                                        EstimatedFinishDtm = c != null ? c.EstimatedFinishDtm : null
                                    }).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving bay status: " + ex.Message);
            }
        }

        // 3. Create Reservation (Default: Pending)
        public async Task<(bool success, string message, int resKy)> CreateReservationAsync(CreateReservationDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            
            try
            {
                // Simple overlap check
                bool overlap = await db.BayReservations.AnyAsync(r => 
                    r.BayKy == dto.BayKy && !r.fInAct && r.ResStatus != "Cancelled" &&
                    ((dto.FromDtm >= r.FromDtm && dto.FromDtm < r.ToDtm) ||
                     (dto.ToDtm > r.FromDtm && dto.ToDtm <= r.ToDtm) ||
                     (dto.FromDtm <= r.FromDtm && dto.ToDtm >= r.ToDtm)));
    
                if (overlap) return (false, "Slot already reserved/requested", 0);
    
                var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
                
                var res = new BayReservation
                {
                    BayKy = dto.BayKy,
                    VehicleKy = dto.VehicleKy,
                    FromDtm = dto.FromDtm,
                    ToDtm = dto.ToDtm,
                    ResType = dto.ResType,
                    ResStatus = "Pending", // Needs Admin Approval
                    fInAct = false,
                    CKy = _userContext.CompanyKey,
                    EntUsrKy = userKey ?? 0,
                    EntDtm = DateTime.Now
                };
    
                db.BayReservations.Add(res);
                await db.SaveChangesAsync();
                return (true, "Reservation created successfully, pending approval", res.ResKy);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, 0);
            }
        }

        // 4. Update Reservation (Approve mainly)
        public async Task<(bool success, string message)> UpdateReservationStatusAsync(int resKy, string status)
        {
            using var db = await _factory.CreateDbContextAsync();
            try
            {
                var res = await db.BayReservations.FindAsync(resKy);
                if (res == null) return (false, "Reservation not found");
                
                res.ResStatus = status; // e.g. Approved, Cancelled
                await db.SaveChangesAsync();
                return (true, "Status updated");
            }
             catch (Exception ex)
            {
                return (false, "Error: " + ex.Message);
            }
        }

        // 5. Update Bay Control (Occupancy)
        public async Task<(bool success, string message)> UpdateBayControlAsync(UpdateBayControlDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();

            try
            {
                var control = await db.BayControls.FirstOrDefaultAsync(c => c.BayKy == dto.BayKy); // One-to-one
                var bay = await db.Bays.FindAsync(dto.BayKy);
    
                if (control == null)
                {
                    if (bay == null) return (false, "Bay not found");
                    control = new BayControl
                    {
                        BayKy = dto.BayKy,
                        BayCd = bay.BayCd,
                        CKy = _userContext.CompanyKey
                    };
                    db.BayControls.Add(control);
                }
    
                control.IsBayOccupied = dto.IsOccupied;
                control.CurrentVehicleKy = dto.VehicleKy;
                control.CurrentActivity = dto.CurrentActivity;
                control.EstimatedFinishDtm = dto.EstimatedFinishDtm;
                control.LastUpdDtm = DateTime.Now;
    
                await db.SaveChangesAsync();
                return (true, "Bay status updated");
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message);
            }
        }

        // 6. Get Bays with ReservationAvailable = 1
        public async Task<List<BayDto>> GetReservableBaysAsync()
        {
             using var db = await _factory.CreateDbContextAsync();
             try
             {
                 return await db.Bays
                     .Where(b => !b.fInAct && b.IsReservationAvailable)
                     .Select(b => new BayDto 
                     {
                         BayKy = b.BayKy,
                         BayCd = b.BayCd,
                         BayNm = b.BayNm,
                         IsReservationAvailable = b.IsReservationAvailable,
                         Description = b.Description
                     }).ToListAsync();
             }
             catch (Exception ex)
             {
                 throw new Exception("Error retrieving reservable bays: " + ex.Message);
             }
        }

        // 7. Get All Reservations (Filter Optional)
        public async Task<List<ReservationDto>> GetReservationsAsync(string? status, DateTime? date)
        {
            using var db = await _factory.CreateDbContextAsync();
            try
            {
                var query = db.BayReservations.Where(r => !r.fInAct);
    
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(r => r.ResStatus == status);
    
                if (date.HasValue)
                    query = query.Where(r => r.FromDtm.Date == date.Value.Date);
    
                return await query.Select(r => new ReservationDto
                {
                    ResKy = r.ResKy,
                    BayKy = r.BayKy,
                    BayNm = "", // Ideally join with Bay to get Name, or client handles it. I'll join.
                    VehicleKy = r.VehicleKy,
                    FromDtm = r.FromDtm,
                    ToDtm = r.ToDtm,
                    ResType = r.ResType,
                    ResStatus = r.ResStatus
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving reservations: " + ex.Message);
            }
        }

        // Delete Reservation
        public async Task<(bool success, string message)> DeleteReservationAsync(int resKy)
        {
            using var db = await _factory.CreateDbContextAsync();
            try
            {
                var res = await db.BayReservations.FindAsync(resKy);
                if (res == null) return (false, "Reservation not found");
                
                res.fInAct = true;
                await db.SaveChangesAsync();
                return (true, "Reservation deleted");
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message);
            }
        }
    }
}
