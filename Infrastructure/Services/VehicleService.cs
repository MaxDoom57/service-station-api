using Application.DTOs.Vehicle;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class VehicleService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;

        public VehicleService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext)
        {
            _dispatcher   = dispatcher;
            _userContext  = userContext;
        }

        public async Task<(bool success, string message, int statusCode)> RegisterVehicleAsync(CreateVehicleRequestDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.RegisterVehicle,
                payload:    dto);

            if (!result.Success)
                return (false, result.Error ?? "Agent error", 500);

            return (true, "Vehicle registered successfully", 201);
        }

        public async Task<(bool success, string message)> UpdateVehicleAsync(CreateVehicleRequestDto dto)
        {
            if (!dto.VehicleKy.HasValue) return (false, "Vehicle Key is required");

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateVehicle,
                payload:    dto);

            if (!result.Success)
                return (false, result.Error ?? "Agent error");

            return (true, "Vehicle updated successfully");
        }

        public async Task<(bool success, string message)> DeleteVehicleAsync(int vehicleKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteVehicle,
                payload:    new { vehicleKy });

            if (!result.Success)
                return (false, result.Error ?? "Agent error");

            return (true, "Vehicle deleted successfully");
        }

        public async Task<List<VehicleListItemDto>> GetActiveVehiclesAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetActiveVehicles,
                payload:    new { });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<VehicleListItemDto>>() ?? new();
        }

        public async Task<List<VehicleDetailDto>> GetAllVehiclesDetailedAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAllVehiclesDetailed,
                payload:    new { });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<VehicleDetailDto>>() ?? new();
        }

        public async Task<VehicleDetailDto?> GetVehicleDetailsAsync(int vehicleKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetVehicleDetails,
                payload:    new { vehicleKy });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<VehicleDetailDto?>();
        }
    }
}
