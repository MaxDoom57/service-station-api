using Application.DTOs.User;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;

        public UserController(UserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveUsers()
        {
            try
            {
                var users = await _service.GetActiveUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch users",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var result = await _service.CreateUserAsync(dto);

                if (!result.success)
                    return BadRequest(new { message = result.message });

                return Ok(new { message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to create user",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{usrKy}")]
        public async Task<IActionResult> ChangePassword(int usrKy,[FromBody] ChangePasswordDto dto)
        {
            try
            {
                var result = await _service.ChangePasswordAsync(usrKy, dto);

                if (!result.success)
                    return BadRequest(new { message = result.message });

                return Ok(new { message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to change password",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{usrKy}")]
        public async Task<IActionResult> DeleteUser(int usrKy)
        {
            try
            {
                var result = await _service.DeleteUserAsync(usrKy);

                if (!result.success)
                    return NotFound(new { message = result.message });

                return Ok(new { message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to delete user",
                    error = ex.Message
                });
            }
        }
    }
}
