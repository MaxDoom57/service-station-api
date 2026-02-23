using Application.DTOs.BayWorker;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class BayWorkerService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public BayWorkerService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<(bool success, string message, int workerKy)> AddWorkerToBayAsync(CreateBayWorkerDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey) ?? 1;

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.AddWorkerToBay,
                payload:    new { Dto = dto, UserKey = userKey, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return (false, result.Error ?? "Failed to assign worker", 0);
            return (true, "Worker assigned successfully", 0);
        }

        public async Task<List<BayWorkerDto>> GetAllWorkersAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAllBayWorkers,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<BayWorkerDto>>() ?? new();
        }

        public async Task<(bool success, string message)> UpdateWorkerInBayAsync(UpdateBayWorkerDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateWorkerInBay,
                payload:    new { Dto = dto });

            if (!result.Success) return (false, result.Error ?? "Failed to update worker assignment");
            return (true, "Worker assignment updated");
        }

        public async Task<(bool success, string message)> DeleteWorkerFromBayAsync(int bayWorkerKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteWorkerFromBay,
                payload:    new { BayWorkerKy = bayWorkerKy });

            if (!result.Success) return (false, result.Error ?? "Failed to remove worker");
            return (true, "Worker removed from bay");
        }
    }
}
