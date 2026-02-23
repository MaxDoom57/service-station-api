using Application.DTOs.PurchaseOrder;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class PurchaseOrderService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public PurchaseOrderService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        // ------------------------------------------------
        // GET PURCHASE ORDER DETAILS
        // ------------------------------------------------
        public async Task<PurchaseOrderResponseDto?> GetPurchaseOrderAsync(int orderNo, int ordTypKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetPurchaseOrder,
                payload:    new { OrderNo = orderNo, OrdTypKy = ordTypKy, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return null;
            return result.Deserialize<PurchaseOrderResponseDto>();
        }

        // ------------------------------------------------
        // POST - Create New Purchase Order
        // ------------------------------------------------
        public async Task<int> CreatePurchaseOrderAsync(PurchaseOrderSaveDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
            if (userKey == null)
                throw new Exception("User key not found");

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.CreatePurchaseOrder,
                payload:    new { Dto = dto, UserKey = userKey.Value, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Failed to create purchase order");

            return result.Deserialize<int>();
        }

        // ------------------------------------------------
        // PUT - Update Purchase Order
        // ------------------------------------------------
        public async Task UpdatePurchaseOrderAsync(int orderNo, PurchaseOrderSaveDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdatePurchaseOrder,
                payload:    new { OrderNo = orderNo, Dto = dto, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Failed to update purchase order");
        }

        // ------------------------------------------------
        // DELETE - Delete Purchase Order
        // ------------------------------------------------
        public async Task DeletePurchaseOrderAsync(int orderNo)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeletePurchaseOrder,
                payload:    new { OrderNo = orderNo, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Purchase order not found or could not be deleted");
        }
    }
}
