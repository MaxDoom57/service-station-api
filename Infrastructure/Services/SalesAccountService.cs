using Application.DTOs.Invoice;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class SalesAccountService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;

        public SalesAccountService(IAgentJobDispatcher dispatcher, IUserRequestContext userContext)
        {
            _dispatcher  = dispatcher;
            _userContext = userContext;
        }

        public async Task<List<SalesAccountDto>> GetAllAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetSalesAccounts,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<SalesAccountDto>>() ?? new();
        }
    }
}
