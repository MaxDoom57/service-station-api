using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class CommonLookupService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;

        public CommonLookupService(IAgentJobDispatcher dispatcher, IUserRequestContext userContext)
        {
            _dispatcher  = dispatcher;
            _userContext = userContext;
        }

        public async Task<short> GetAccountTypeKeyAsync(string accountType)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAccountTypeKey,
                payload:    new { AccountType = accountType, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception($"Account type '{accountType}' not found in CdMas.");

            return result.Deserialize<short>();
        }

        public async Task<short> GetItemTypeKeyAsync(string itemType)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetItemTypeKey,
                payload:    new { ItemType = itemType, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception($"Item Type '{itemType}' not found");

            return result.Deserialize<short>();
        }

        public async Task<short> GetTranTypeKeyAsync(string tranTypeCode)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetTranTypeKey,
                payload:    new { TranTypeCode = tranTypeCode });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<short>();
        }

        public async Task<short> GetPaymentTermKeyAsync(string paymentTermCode)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetPaymentTermKey,
                payload:    new { PaymentTermCode = paymentTermCode });

            if (!result.Success)
                throw new Exception($"Payment term '{paymentTermCode}' not found in CdMas.");

            return result.Deserialize<short>();
        }

        public async Task<int> GetAddressKeyByAccKyAsync(int accKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAddressKeyByAccKy,
                payload:    new { AccKy = accKy });

            if (!result.Success)
                throw new Exception("Address not found.");

            return result.Deserialize<int>();
        }

        public async Task<vewCdMas?> GetSaleTransactionCodesAsync(int cKy, string ourCode)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: cKy,
                jobType:    AgentJobTypes.GetSaleTransactionCodes,
                payload:    new { CKy = cKy, OurCode = ourCode });

            if (!result.Success) return null;
            return result.Deserialize<vewCdMas>();
        }

        public async Task<int> GetDefaultSalesAccountKeyAsync(short cKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: cKy,
                jobType:    AgentJobTypes.GetDefaultSalesAccountKey,
                payload:    new { CKy = cKy });

            if (!result.Success) return 0;
            return result.Deserialize<int>();
        }

        public async Task<int> GetTrnKyByTrnNoAsync(int trnNo)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetTrnKyByTrnNo,
                payload:    new { TrnNo = trnNo });

            if (!result.Success) return 0;
            return result.Deserialize<int>();
        }

        public async Task<string?> GetCompanyNameByCKyAsync(int cky)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: cky,
                jobType:    AgentJobTypes.GetCompanyNameByCKy,
                payload:    new { CKy = cky });

            if (!result.Success) return null;
            return result.Deserialize<string?>();
        }

        public async Task<int> GetTrnTypKyAsync(string ourCode)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetTrnTypKy,
                payload:    new { OurCode = ourCode });

            if (!result.Success) return 0;
            return result.Deserialize<int>();
        }

        public async Task<short> GetPaymentModeKeyAsync(int paymentTermKey)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetPaymentModeKey,
                payload:    new { PaymentTermKey = paymentTermKey });

            if (!result.Success)
                throw new Exception($"Payment mode not found for PaymentTermKey {paymentTermKey}");

            return result.Deserialize<short>();
        }

        public async Task<short> GetCodeTypeKeyAsync(string conCd)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetCodeTypeKey,
                payload:    new { ConCd = conCd });

            if (!result.Success)
                throw new Exception($"Code type not found for code type {conCd}");

            return result.Deserialize<short>();
        }

        public async Task<int> GetTranNumberLastAsync(int companyKey, string ourCode)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: companyKey,
                jobType:    AgentJobTypes.GetTranNumberLast,
                payload:    new { CompanyKey = companyKey, OurCode = ourCode });

            if (!result.Success) throw new Exception(result.Error ?? "Agent error");
            return result.Deserialize<int>();
        }

        public async Task IncrementTranNumberLastAsync(int companyKey, string ourCode)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: companyKey,
                jobType:    AgentJobTypes.IncrementTranNumberLast,
                payload:    new { CompanyKey = companyKey, OurCode = ourCode });

            if (!result.Success)
                throw new Exception("Transaction number record not found.");
        }

        public async Task<int> GetAmt1AccKyAsync(string ourCd, int companyKey)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: companyKey,
                jobType:    AgentJobTypes.GetAmt1AccKy,
                payload:    new { OurCd = ourCd, CompanyKey = companyKey });

            if (!result.Success) return 0;
            return result.Deserialize<int>();
        }

        public async Task<List<AccessLevelDto>> GetAccessLevelAsync(string userId)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAccessLevel,
                payload:    new { UserId = userId, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<AccessLevelDto>>() ?? new();
        }
    }
}
