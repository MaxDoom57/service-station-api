using Application.DTOs.Order;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class OrderManagementService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public OrderManagementService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<(bool success, string message, int ordKy)> CreateOrderAsync(CreateOrderDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey) ?? 0;

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.CreateOrder,
                payload:    new { Dto = dto, UserKey = userKey, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                return (false, result.Error ?? "Error creating order", 0);

            var ordKy = result.Deserialize<int>();
            return (true, "Order created successfully", ordKy);
        }

        public async Task<(bool success, string message)> UpdateOrderAsync(UpdateOrderDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey) ?? 0;

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateOrder,
                payload:    new { Dto = dto, UserKey = userKey, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                return (false, result.Error ?? "Error updating order");

            return (true, "Order updated successfully");
        }

        public async Task<(bool success, string message)> DeleteOrderAsync(int ordKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteOrder,
                payload:    new { OrdKy = ordKy, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                return (false, result.Error ?? "Error deleting order");

            return (true, "Order deleted successfully");
        }

        public async Task<List<OrderListDto>> GetAllOrdersAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAllOrders,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success) throw new Exception(result.Error ?? "Agent error");
            return result.Deserialize<List<OrderListDto>>() ?? new();
        }

        public async Task<OrderDetailResponseDto?> GetOrderByOrderNoAsync(int ordNo)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetOrderByOrderNo,
                payload:    new { OrdNo = ordNo, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return null;
            return result.Deserialize<OrderDetailResponseDto>();
        }

        public async Task<OrderDetailResponseDto?> GetOrderByKeyAsync(int ordKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetOrderByKey,
                payload:    new { OrdKy = ordKy, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return null;
            return result.Deserialize<OrderDetailResponseDto>();
        }
    }
}
