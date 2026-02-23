using Application.DTOs.Bay;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class BayService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public BayService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<List<BayDto>> GetActiveBaysAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetActiveBays,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<BayDto>>() ?? new();
        }

        public async Task<(bool success, string message)> AddBayAsync(CreateBayDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
            if (userKey == null) return (false, "User key not found");

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.AddBay,
                payload:    new { Dto = dto, UserKey = userKey.Value, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return (false, result.Error ?? "Failed to add Bay");
            return (true, "Bay added successfully");
        }

        public async Task<(bool success, string message)> UpdateBayAsync(UpdateBayDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateBay,
                payload:    new { Dto = dto });

            if (!result.Success) return (false, result.Error ?? "Failed to update Bay");
            return (true, "Bay updated successfully");
        }

        public async Task<(bool success, string message)> DeleteBayAsync(int bayKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteBay,
                payload:    new { BayKy = bayKy });

            if (!result.Success) return (false, result.Error ?? "Failed to delete Bay");
            return (true, "Bay deleted successfully");
        }
    }
}
