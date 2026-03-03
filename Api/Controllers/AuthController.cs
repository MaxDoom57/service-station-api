using Application.DTOs.Auth;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly ITokenBlacklistService _tokenBlacklist;
        private readonly IUserRequestContext _userRequestContext;
        private readonly CommonLookupService _lookupService;
        private readonly ITokenService _tokenService;
        private readonly ITokenActivityService _tokenActivity;
        private readonly IConfiguration _config;

        public AuthController(
            ILoginService loginService,
            ITokenBlacklistService tokenBlacklist,
            IUserRequestContext userRequestContext,
            CommonLookupService lookupService,
            ITokenService tokenService,
            ITokenActivityService tokenActivity,
            IConfiguration config)
        {
            _loginService = loginService;
            _tokenBlacklist = tokenBlacklist;
            _userRequestContext = userRequestContext;
            _lookupService = lookupService;
            _tokenService = tokenService;
            _tokenActivity = tokenActivity;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _loginService.LoginAsync(request);

                if (result == null)
                    return Unauthorized("Invalid user id or password.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes the JWT token using the current Bearer token.
        /// - The old token must have a valid signature (tamper-proof check).
        /// - The user must have made at least one request within the last hour
        ///   (sliding-window activity check via ITokenActivityService).
        /// - On success the old token is blacklisted and a brand-new 1-hour token is returned.
        /// As long as the client calls this endpoint (or any other API endpoint) within
        /// every 1-hour window, the session never expires.
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var currentToken = HttpContext.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(currentToken))
                    return BadRequest("Token missing.");

                if (_tokenBlacklist.IsTokenBlacklisted(currentToken))
                    return Unauthorized("Token has been revoked. Please login again.");

                // --- Validate token signature (ValidateLifetime = false to accept recently-expired tokens) ---
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                ClaimsPrincipalResult validationResult;
                try
                {
                    var validationParams = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = false,   // allow expired tokens — activity check handles replay prevention
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidAudience = _config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };

                    var principal = tokenHandler.ValidateToken(currentToken, validationParams, out _);
                    validationResult = new ClaimsPrincipalResult(principal, null);
                }
                catch (Exception ex)
                {
                    return Unauthorized($"Invalid token: {ex.Message}");
                }

                // --- Sliding-window activity check ---
                if (!_tokenActivity.IsActiveWithinWindow(currentToken))
                {
                    return Unauthorized("Session expired due to inactivity. Please login again.");
                }

                // --- Issue new token carrying the same application claims ---
                var newToken = await _tokenService.GenerateTokenFromClaims(validationResult.Principal.Claims);

                // --- Token rotation: blacklist old, register new ---
                _tokenBlacklist.BlacklistToken(currentToken, DateTime.UtcNow.AddHours(1));
                _tokenActivity.Remove(currentToken);
                _tokenActivity.Touch(newToken);

                return Ok(new LoginResponseDto
                {
                    Token = newToken,
                    ExpireAt = DateTime.UtcNow.AddHours(1)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                    return BadRequest("Token missing");

                if (_tokenBlacklist.IsTokenBlacklisted(token))
                    return BadRequest("Token already blacklisted");

                var expiration = DateTime.UtcNow.AddHours(1);
                _tokenBlacklist.BlacklistToken(token, expiration);
                _tokenActivity.Remove(token);

                return Ok("Logged out successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("accessLevel")]
        public async Task<IActionResult> GetAccessLevel()
        {
            try
            {
                var accessList = await _lookupService.GetAccessLevelAsync(_userRequestContext.UserId);

                if (accessList.Count < 2)
                    throw new Exception("Access list does not contain both dashboard and pos menu entries");

                var dashboard = accessList[0];
                var posMenu = accessList[1];

                string dashboardAccess =
                    $"{ToBit(dashboard.fAcs)}{ToBit(dashboard.fNew)}{ToBit(dashboard.fUpdt)}{ToBit(dashboard.fDel)}{ToBit(dashboard.fSp)}";

                string posMenuAccess =
                    $"{ToBit(posMenu.fAcs)}{ToBit(posMenu.fNew)}{ToBit(posMenu.fUpdt)}{ToBit(posMenu.fDel)}{ToBit(posMenu.fSp)}";

                return Ok(new { AccessLevel = $"{dashboardAccess}, {posMenuAccess}" });
            }
            catch (Exception ex)
            {
                var fullError = new
                {
                    Message = ex.Message,
                    Type = ex.GetType().FullName,
                    StackTrace = ex.StackTrace,
                    InnerMessage = ex.InnerException?.Message,
                    InnerStack = ex.InnerException?.StackTrace
                };

                return StatusCode(500, fullError);
            }
        }

        private static int ToBit(bool value) => value ? 1 : 0;
    }

    // Helper record — lives in namespace scope so it is accessible from the method scope
    internal record ClaimsPrincipalResult(
        System.Security.Claims.ClaimsPrincipal Principal,
        string? Error);
}
