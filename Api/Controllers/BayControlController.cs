using Application.DTOs.BayControl;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/baycontrol")]
    [Authorize]
    public class BayControlController : ControllerBase
    {
        private readonly BayControlService _service;

        public BayControlController(BayControlService service)
        {
            _service = service;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableBaysNow()
        {
            try
            {
                return Ok(await _service.GetAvailableBaysNowAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetAllBaysStatus()
        {
            try
            {
                return Ok(await _service.GetAllBaysStatusAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("reservable")]
        public async Task<IActionResult> GetReservableBays()
        {
            try
            {
                return Ok(await _service.GetReservableBaysAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reservation")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
        {
            try
            {
                var result = await _service.CreateReservationAsync(dto);
                if (!result.success) return BadRequest(result.message);
                return Ok(new { result.message, result.resKy });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("reservation/{resKy}/status")]
        public async Task<IActionResult> UpdateReservationStatus(int resKy, [FromQuery] string status)
        {
            try
            {
                var result = await _service.UpdateReservationStatusAsync(resKy, status);
                return result.success ? Ok(result.message) : BadRequest(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
        [HttpDelete("reservation/{resKy}")]
        public async Task<IActionResult> DeleteReservation(int resKy)
        {
            try
            {
                var result = await _service.DeleteReservationAsync(resKy);
                return result.success ? Ok(result.message) : BadRequest(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateBayControl([FromBody] UpdateBayControlDto dto)
        {
            try
            {
                var result = await _service.UpdateBayControlAsync(dto);
                return result.success ? Ok(result.message) : BadRequest(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("reservations")]
        public async Task<IActionResult> GetReservations([FromQuery] string? status, [FromQuery] DateTime? date)
        {
            try
            {
                return Ok(await _service.GetReservationsAsync(status, date));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
