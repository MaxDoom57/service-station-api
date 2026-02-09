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
            return Ok(await _service.GetAvailableBaysNowAsync());
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetAllBaysStatus()
        {
            return Ok(await _service.GetAllBaysStatusAsync());
        }

        [HttpGet("reservable")]
        public async Task<IActionResult> GetReservableBays()
        {
            return Ok(await _service.GetReservableBaysAsync());
        }

        [HttpPost("reservation")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
        {
            var result = await _service.CreateReservationAsync(dto);
            if (!result.success) return BadRequest(result.message);
            return Ok(new { result.message, result.resKy });
        }

        [HttpPut("reservation/{resKy}/status")]
        public async Task<IActionResult> UpdateReservationStatus(int resKy, [FromQuery] string status)
        {
            var result = await _service.UpdateReservationStatusAsync(resKy, status);
            return result.success ? Ok(result.message) : BadRequest(result.message);
        }

        [HttpDelete("reservation/{resKy}")]
        public async Task<IActionResult> DeleteReservation(int resKy)
        {
            var result = await _service.DeleteReservationAsync(resKy);
            return result.success ? Ok(result.message) : BadRequest(result.message);
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateBayControl([FromBody] UpdateBayControlDto dto)
        {
            var result = await _service.UpdateBayControlAsync(dto);
            return result.success ? Ok(result.message) : BadRequest(result.message);
        }

        [HttpGet("reservations")]
        public async Task<IActionResult> GetReservations([FromQuery] string? status, [FromQuery] DateTime? date)
        {
            return Ok(await _service.GetReservationsAsync(status, date));
        }
    }
}
