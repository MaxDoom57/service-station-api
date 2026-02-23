using Application.DTOs.User;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

namespace Infrastructure.Services
{
    public class UserService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly CommonLookupService _lookup;
        private readonly IUserKeyService _userKeyService;
        private readonly IValidationService _validator;

        public UserService(
        IDynamicDbContextFactory factory,
        IUserRequestContext userContext,
        CommonLookupService lookup,
        IUserKeyService userKeyService,
        IValidationService validator)
        {
            _factory = factory;
            _userContext = userContext;
            _lookup = lookup;
            _userKeyService = userKeyService;
            _validator = validator;
        }

        public async Task<List<UserLookupDto>> GetActiveUsersAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            var users = await db.UsrMas
                .Where(x => x.fInAct == false)
                .OrderBy(x => x.UsrId)
                .ToListAsync();

            return users.Select(x => new UserLookupDto
            {
                UsrKy = x.UsrKy,
                UsrId = x.UsrId
            }).ToList();
        }


        public async Task<(bool success, string message)> CreateUserAsync(CreateUserDto dto)
        {
            if (dto.NewPwd != dto.ConfirmPwd)
                return (false, "Confirmation of password is incorrect. Please re-enter.");

            try
            {
                using var db = await _factory.CreateDbContextAsync();
                using var tx = await db.Database.BeginTransactionAsync();

                if (await db.UsrMas.AnyAsync(x => x.UsrId == dto.UsrId))
                    return (false, "The User ID already exists!");

                var user = new UsrMas
                {
                    UsrNm = dto.UsrNm,
                    UsrId = dto.UsrId,
                    Pwd = EncryptPasswordSha256(dto.NewPwd),
                    PwdTip = dto.PwdTip,
                    fInAct = false
                };

                db.UsrMas.Add(user);
                await db.SaveChangesAsync();
                await tx.CommitAsync();

                return (true, $"User : {dto.UsrId} is successfully added.");
            }
            catch (Exception ex)
            {
                return (false, ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<(bool success, string message)> ChangePasswordAsync(int usrKy,ChangePasswordDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();

            var user = await db.UsrMas
                .FirstOrDefaultAsync(x => x.UsrKy == usrKy);

            if (user == null)
                return (false, "This user is not properly registered!");

            var oldPwdHash = EncryptPasswordSha256(dto.OldPwd);

            if (user.Pwd != oldPwdHash)
                return (false, "The old password you typed is incorrect");

            if (dto.NewPwd != dto.ConfirmPwd)
                return (false, "Confirmation of password is incorrect. Please re-enter!");

            user.Pwd = EncryptPasswordSha256(dto.NewPwd);
            user.PwdTip = dto.PwdTip;

            await db.SaveChangesAsync();

            return (true, "Password successfully changed.");
        }

        public async Task<(bool success, string message)> DeleteUserAsync(int usrKy)
        {
            using var db = await _factory.CreateDbContextAsync();

            var user = await db.UsrMas
                .FirstOrDefaultAsync(x => x.UsrKy == usrKy);

            if (user == null)
                return (false, "User not found");

            user.fInAct = true;
            user.Status = "D";

            await db.SaveChangesAsync();

            return (true, "User successfully deleted");
        }


        private string EncryptPasswordSha256(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
