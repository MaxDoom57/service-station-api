using Application.DTOs.Codes;
using Application.DTOs.Lookups;
using Application.Interfaces;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class CodeService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public CodeService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<List<CodeTypes>> GetCodeTypesAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetCodeTypes,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<CodeTypes>>() ?? new();
        }

        public async Task<List<CodeByTypeDto>> GetCodesByTypeAsync(GetCodesByTypeRequestDto request)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetCodesByType,
                payload:    new { Request = request, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<CodeByTypeDto>>() ?? new();
        }

        public async Task<CodeResponseDto> CreateCodeAsync(CreateCodeDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.CreateCode,
                payload:    new { Dto = dto, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new InvalidOperationException(result.Error ?? "Failed to create code");

            return result.Deserialize<CodeResponseDto>()
                   ?? new CodeResponseDto { Message = "Code created successfully" };
        }

        public async Task<CodeResponseDto> UpdateCodeAsync(int cdKy, UpdateCodeDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateCode,
                payload:    new { CdKy = cdKy, Dto = dto });

            if (!result.Success)
                throw new InvalidOperationException(result.Error ?? "Failed to update code");

            return result.Deserialize<CodeResponseDto>()
                   ?? new CodeResponseDto { CdKy = cdKy, Message = "Code updated successfully" };
        }

        public async Task<CodeResponseDto> DeleteCodeAsync(int cdKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteCode,
                payload:    new { CdKy = cdKy });

            if (!result.Success)
                throw new InvalidOperationException(result.Error ?? "Failed to delete code");

            return result.Deserialize<CodeResponseDto>()
                   ?? new CodeResponseDto { CdKy = cdKy, Message = "Code deleted successfully" };
        }
    }
}
