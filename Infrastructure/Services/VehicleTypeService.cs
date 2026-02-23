using Application.DTOs.VehicleType;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class VehicleTypeService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public VehicleTypeService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<List<VehicleTypeDto>> GetVehicleTypesAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetVehicleTypes,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<VehicleTypeDto>>() ?? new();
        }

        public async Task<(bool success, string message)> AddVehicleTypeAsync(CreateVehicleTypeDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
            if (userKey == null) return (false, "User key not found");

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.AddVehicleType,
                payload:    new { Dto = dto, UserKey = userKey.Value, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return (false, result.Error ?? "Failed to add Vehicle Type");
            return (true, "Vehicle Type added successfully");
        }

        public async Task<(bool success, string message)> UpdateVehicleTypeAsync(CreateVehicleTypeDto dto)
        {
            if (!dto.CdKy.HasValue) return (false, "Vehicle Type Key is required");

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateVehicleType,
                payload:    new { Dto = dto, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return (false, result.Error ?? "Failed to update Vehicle Type");
            return (true, "Vehicle Type updated successfully");
        }

        public async Task<(bool success, string message)> DeleteVehicleTypeAsync(int cdKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteVehicleType,
                payload:    new { CdKy = cdKy });

            if (!result.Success) return (false, result.Error ?? "Failed to delete Vehicle Type");
            return (true, "Vehicle Type deleted successfully");
        }
    }
}
