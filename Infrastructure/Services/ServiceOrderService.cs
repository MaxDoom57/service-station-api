using Application.DTOs.ServiceOrder;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class ServiceOrderService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public ServiceOrderService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<(bool success, string message, int ordKy)> CreateServiceOrderAsync(CreateServiceOrderDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey) ?? 0;

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.CreateServiceOrder,
                payload:    new { Dto = dto, UserKey = userKey, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return (false, result.Error ?? "Failed to create service order", 0);

            var ordKy = result.Deserialize<int>();
            return (true, "Service Order Created", ordKy);
        }

        public async Task<(bool success, string message)> AddServiceItemAsync(AddServiceItemDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey) ?? 0;

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.AddServiceItem,
                payload:    new { Dto = dto, UserKey = userKey });

            if (!result.Success) return (false, result.Error ?? "Failed to add service item");
            return (true, "Item added, pending approval");
        }

        public async Task<(bool success, string message)> ApproveServiceItemAsync(ApproveServiceItemDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.ApproveServiceItem,
                payload:    new { Dto = dto });

            if (!result.Success) return (false, result.Error ?? "Failed to approve service item");
            return (true, dto.IsApproved ? "Item approved" : "Item rejected/removed");
        }

        public async Task<(bool success, string message)> UpdateItemStatusAsync(UpdateItemStatusDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateServiceItemStatus,
                payload:    new { Dto = dto });

            if (!result.Success) return (false, result.Error ?? "Failed to update item status");
            return (true, "Status updated");
        }

        public async Task<List<ServiceOrderDto>> GetServiceOrdersAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetServiceOrders,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<ServiceOrderDto>>() ?? new();
        }

        public async Task<ServiceOrderDto?> GetServiceOrderDetailsAsync(int ordKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetServiceOrderDetails,
                payload:    new { OrdKy = ordKy });

            if (!result.Success) return null;
            return result.Deserialize<ServiceOrderDto>();
        }
    }
}
