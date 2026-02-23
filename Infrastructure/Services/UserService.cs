using Application.DTOs.User;
using Application.Interfaces;
using Shared.Constants;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class UserService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public UserService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        public async Task<List<UserLookupDto>> GetActiveUsersAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetActiveUsers,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                throw new Exception(result.Error ?? "Agent error");

            return result.Deserialize<List<UserLookupDto>>() ?? new();
        }

        public async Task<(bool success, string message)> CreateUserAsync(CreateUserDto dto)
        {
            if (dto.NewPwd != dto.ConfirmPwd)
                return (false, "Confirmation of password is incorrect. Please re-enter.");

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.CreateUser,
                payload:    new
                {
                    Dto         = dto,
                    HashedPwd   = EncryptPasswordSha256(dto.NewPwd),
                    CompanyKey  = _userContext.CompanyKey
                });

            if (!result.Success) return (false, result.Error ?? "Failed to create user");
            return (true, $"User : {dto.UsrId} is successfully added.");
        }

        public async Task<(bool success, string message)> ChangePasswordAsync(int usrKy, ChangePasswordDto dto)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.ChangePassword,
                payload:    new
                {
                    UsrKy       = usrKy,
                    OldPwdHash  = EncryptPasswordSha256(dto.OldPwd),
                    NewPwdHash  = EncryptPasswordSha256(dto.NewPwd),
                    ConfirmPwd  = dto.ConfirmPwd,
                    PwdTip      = dto.PwdTip,
                    CompanyKey  = _userContext.CompanyKey
                });

            if (!result.Success) return (false, result.Error ?? "Failed to change password");
            return (true, "Password successfully changed.");
        }

        public async Task<(bool success, string message)> DeleteUserAsync(int usrKy)
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteUser,
                payload:    new { UsrKy = usrKy });

            if (!result.Success) return (false, result.Error ?? "Failed to delete user");
            return (true, "User successfully deleted");
        }

        private string EncryptPasswordSha256(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes     = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);
            var sb        = new StringBuilder();
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
