using Application.DTOs.Lookups;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class LookupService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;

        public LookupService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext)
        {
            _dispatcher  = dispatcher;
            _userContext = userContext;
        }

        public async Task<List<ItemCategory1Dto>> GetItemCategory1Async()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetItemCategory1,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success) throw new Exception(result.Error ?? "Agent error");
            return result.Deserialize<List<ItemCategory1Dto>>() ?? new();
        }

        public async Task<List<ItemCategory2Dto>> GetItemCategory2Async()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetItemCategory2,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success) throw new Exception(result.Error ?? "Agent error");
            return result.Deserialize<List<ItemCategory2Dto>>() ?? new();
        }

        public async Task<List<ItemCategory3Dto>> GetItemCategory3Async()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetItemCategory3,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success) throw new Exception(result.Error ?? "Agent error");
            return result.Deserialize<List<ItemCategory3Dto>>() ?? new();
        }

        public async Task<List<ItemCategory4Dto>> GetItemCategory4Async()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetItemCategory4,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success) throw new Exception(result.Error ?? "Agent error");
            return result.Deserialize<List<ItemCategory4Dto>>() ?? new();
        }

        public async Task<int> GetLastTransactionNoAsync(GetLastTrnNoRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OurCd))
                throw new ArgumentException("OurCd is required");

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetLastTransactionNo,
                payload:    new { OurCd = request.OurCd, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) throw new Exception(result.Error ?? "Agent error");
            return result.Deserialize<int>();
        }
    }
}
