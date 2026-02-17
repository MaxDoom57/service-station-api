using Application.DTOs.Auth;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

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

        public AuthController(
            ILoginService loginService,
            ITokenBlacklistService tokenBlacklist,
            IUserRequestContext userRequestContext,
            CommonLookupService lookupService)
        {
            _loginService = loginService;
            _tokenBlacklist = tokenBlacklist;
            _userRequestContext = userRequestContext;
            _lookupService = lookupService;
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

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                    return BadRequest("Token missing");

                if (_tokenBlacklist.IsTokenBlacklisted(token))
                    return BadRequest("Token already blacklisted");

                Console.WriteLine($"UserId = {_userRequestContext.UserId}");
                Console.WriteLine($"CompanyKey = {_userRequestContext.CompanyKey}");
                Console.WriteLine($"ProjectKey = {_userRequestContext.ProjectKey}");

                // Blacklist the token for the remaining lifetime
                var expiration = DateTime.UtcNow.AddHours(1);
                _tokenBlacklist.BlacklistToken(token, expiration);

                return Ok("Logged out successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private static int ToBit(bool value)
        {
            return value ? 1 : 0;
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

                Console.WriteLine(accessList[0]);
                Console.WriteLine(accessList[1]);

                string dashboardAccess =
                    $"{ToBit(dashboard.fAcs)}{ToBit(dashboard.fNew)}{ToBit(dashboard.fUpdt)}{ToBit(dashboard.fDel)}{ToBit(dashboard.fSp)}";

                string posMenuAccess =
                    $"{ToBit(posMenu.fAcs)}{ToBit(posMenu.fNew)}{ToBit(posMenu.fUpdt)}{ToBit(posMenu.fDel)}{ToBit(posMenu.fSp)}";

                string accessLevelString = $"{dashboardAccess}, {posMenuAccess}";

                return Ok(new { AccessLevel = accessLevelString });
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
    }
}
