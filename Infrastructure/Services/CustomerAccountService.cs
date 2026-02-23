using Application.DTOs.Invoice;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class CustomerAccountService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;

        public CustomerAccountService(IAgentJobDispatcher dispatcher, IUserRequestContext userContext)
        {
            _dispatcher   = dispatcher;
            _userContext  = userContext;
        }

        public async Task<List<CustomerAccountDto>> GetAllAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetCustomerAccounts,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<CustomerAccountDto>>() ?? new();
        }
    }
}
