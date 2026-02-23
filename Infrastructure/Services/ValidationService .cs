using Application.Interfaces;
using Shared.Constants;

namespace Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;

        public ValidationService(IAgentJobDispatcher dispatcher, IUserRequestContext userContext)
        {
            _dispatcher  = dispatcher;
            _userContext = userContext;
        }

        public async Task<bool> IsExistCompanyKey(int companyKey)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: companyKey,
                jobType:    AgentJobTypes.ValidateCompanyKey,
                payload:    new { CompanyKey = companyKey });
            return result.Success && result.Deserialize<bool>();
        }

        public async Task<bool> IsExistItemCode(string itemCode)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.ValidateItemCode,
                payload:    new { ItemCode = itemCode });
            return result.Success && result.Deserialize<bool>();
        }

        public async Task<bool> IsExistItemType(string itemType)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.ValidateItemType,
                payload:    new { ItemType = itemType });
            return result.Success && result.Deserialize<bool>();
        }

        public async Task<bool> IsValidUnitKey(short unitKey)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.ValidateUnitKey,
                payload:    new { UnitKey = unitKey });
            return result.Success && result.Deserialize<bool>();
        }

        public async Task<bool> IsValidUserKey(int userKey)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.ValidateUserKey,
                payload:    new { UserKey = userKey });
            return result.Success && result.Deserialize<bool>();
        }

        public async Task<bool> IsExistAdrNm(string adrNm)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.ValidateAdrNm,
                payload:    new { AdrNm = adrNm });
            return result.Success && result.Deserialize<bool>();
        }

        public async Task<bool> IsValidTranDate(DateTime trnDt)
        {
            var currentDate = DateTime.UtcNow.Date.ToLocalTime();
            return trnDt.Date >= currentDate.AddDays(1) && trnDt.Date <= currentDate.AddDays(-60);
        }
    }
}
