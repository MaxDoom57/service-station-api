using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;


namespace Infrastructure.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public LoginService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(dto.UserId, dto.CompanyKey);
                if (user == null) return null;

                if (!PasswordHasher.Verify(dto.Password, user.Pwd))
                    return null;

                var token = await _tokenService.GenerateToken(user, dto.ProjectKey);

                return new LoginResponseDto
                {
                    Token = token,
                    ExpireAt = DateTime.UtcNow.AddHours(1),
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Login Failed: " + ex.Message);
            }
        }
    }
}
