using Application.DTOs.BayControl;
using Application.DTOs.Bay;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class BayControlService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public BayControlService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<List<AvailableBayDto>> GetAvailableBaysNowAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAvailableBaysNow,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<AvailableBayDto>>() ?? new();
        }

        public async Task<List<BayStatusDto>> GetAllBaysStatusAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAllBaysStatus,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<BayStatusDto>>() ?? new();
        }

        public async Task<(bool success, string message, int resKy)> CreateReservationAsync(CreateReservationDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.CreateBayReservation,
                payload:    new { Dto = dto, UserKey = userKey ?? 0, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return (false, result.Error ?? "Failed to create reservation", 0);
            return (true, "Reservation created successfully, pending approval", 0);
        }

        public async Task<(bool success, string message)> UpdateReservationStatusAsync(int resKy, string status)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateBayReservationStatus,
                payload:    new { ResKy = resKy, Status = status });

            if (!result.Success) return (false, result.Error ?? "Failed to update reservation status");
            return (true, "Status updated");
        }

        public async Task<(bool success, string message)> UpdateBayControlAsync(UpdateBayControlDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateBayControl,
                payload:    new { Dto = dto, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return (false, result.Error ?? "Failed to update bay control");
            return (true, "Bay status updated");
        }

        public async Task<List<BayDto>> GetReservableBaysAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetReservableBays,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<BayDto>>() ?? new();
        }

        public async Task<List<ReservationDto>> GetReservationsAsync(string? status, DateTime? date)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetBayReservations,
                payload:    new { Status = status, Date = date, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<ReservationDto>>() ?? new();
        }

        public async Task<(bool success, string message)> DeleteReservationAsync(int resKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteBayReservation,
                payload:    new { ResKy = resKy });

            if (!result.Success) return (false, result.Error ?? "Failed to delete reservation");
            return (true, "Reservation deleted");
        }
    }
}
