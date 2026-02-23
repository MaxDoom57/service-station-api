using Application.DTOs.Reservation;
using Application.DTOs.Vehicle;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class ReservationService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;
        private readonly VehicleService _vehicleService;

        public ReservationService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService,
            VehicleService vehicleService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
            _vehicleService = vehicleService;
        }

        public async Task<(bool success, string message, int resKy)> CreateReservationAsync(CreateFullReservationDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.CreateReservation,
                payload:    new { Dto = dto, UserKey = userKey ?? 0, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return (false, result.Error ?? "Failed to create reservation", 0);
            return (true, "Reservation placed successfully, awaiting approval.", 0);
        }

        public async Task<(bool success, string message)> UpdateReservationAsync(int resKy, CreateFullReservationDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateReservation,
                payload:    new { ResKy = resKy, Dto = dto });

            if (!result.Success) return (false, result.Error ?? "Failed to update reservation");
            return (true, "Reservation updated");
        }

        public async Task<(bool success, string message)> DeleteReservationAsync(int resKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteReservation,
                payload:    new { ResKy = resKy });

            if (!result.Success) return (false, result.Error ?? "Failed to delete reservation");
            return (true, "Reservation deleted");
        }

        public async Task<(bool success, string message)> ApproveReservationAsync(int resKy, bool approve)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.ApproveReservation,
                payload:    new { ResKy = resKy, Approve = approve });

            if (!result.Success) return (false, result.Error ?? "Failed to process approval");

            string status = approve ? "Approved" : "Cancelled";
            return (true, $"Reservation {status}");
        }

        public async Task<List<ReservationDetailDto>> GetReservationsAsync(string? vehicleId, DateTime? date)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetReservations,
                payload:    new { VehicleId = vehicleId, Date = date, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<ReservationDetailDto>>() ?? new();
        }
    }
}
