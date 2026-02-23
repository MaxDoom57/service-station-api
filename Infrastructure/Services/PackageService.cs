using Application.DTOs.Package;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class PackageService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public PackageService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<List<PackageDto>> GetPackagesAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetPackages,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<PackageDto>>() ?? new();
        }

        public async Task<(bool success, string message)> AddPackageAsync(CreatePackageDto dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
            if (userKey == null) return (false, "User key not found");

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.AddPackage,
                payload:    new { Dto = dto, UserKey = userKey.Value, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return (false, result.Error ?? "Failed to add package");
            return (true, "Package added successfully");
        }

        public async Task<(bool success, string message)> UpdatePackageAsync(CreatePackageDto dto)
        {
            if (!dto.CdKy.HasValue) return (false, "Package Key is required");

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdatePackage,
                payload:    new { Dto = dto });

            if (!result.Success) return (false, result.Error ?? "Failed to update package");
            return (true, "Package updated successfully");
        }

        public async Task<(bool success, string message)> DeletePackageAsync(int cdKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeletePackage,
                payload:    new { CdKy = cdKy });

            if (!result.Success) return (false, result.Error ?? "Failed to delete package");
            return (true, "Package deleted successfully");
        }

        public async Task<PackageDetailDto?> GetPackageDetailsAsync(int cdKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetPackageDetails,
                payload:    new { CdKy = cdKy, CompanyKey = _userContext.CompanyKey });

            if (!result.Success) return null;
            return result.Deserialize<PackageDetailDto>();
        }

        public async Task<List<PackageDetailDto>> GetAllPackagesWithItemsAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAllPackagesWithItems,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<PackageDetailDto>>() ?? new();
        }
    }
}
