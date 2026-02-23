using Application.DTOs.Invoice;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class PaymentTermService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;

        public PaymentTermService(IAgentJobDispatcher dispatcher, IUserRequestContext userContext)
        {
            _dispatcher  = dispatcher;
            _userContext = userContext;
        }

        public async Task<List<PaymentTermDto>> GetAllAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetPaymentTerms,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<PaymentTermDto>>() ?? new();
        }
    }
}
