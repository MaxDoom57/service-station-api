using Application.DTOs.Reservation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/reservation")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly ReservationService _service;

        public ReservationController(ReservationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateFullReservationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateReservationAsync(dto);
            if (!result.success) return BadRequest(result.message);

            return Ok(new { message = result.message, resKy = result.resKy });
        }

        [HttpPut("{resKy}")]
        public async Task<IActionResult> UpdateReservation(int resKy, [FromBody] CreateFullReservationDto dto)
        {
            var result = await _service.UpdateReservationAsync(resKy, dto);
            if (!result.success) return BadRequest(result.message);
            return Ok(result.message);
        }

        [HttpDelete("{resKy}")]
        public async Task<IActionResult> DeleteReservation(int resKy)
        {
            var result = await _service.DeleteReservationAsync(resKy);
            if (!result.success) return BadRequest(result.message);
            return Ok(result.message);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            var result = await _service.GetReservationsAsync(null, null);
            return Ok(result);
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetReservationsByVehicle(string vehicleId)
        {
            var result = await _service.GetReservationsAsync(vehicleId, null);
            return Ok(result);
        }

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetReservationsByDate(DateTime date)
        {
            var result = await _service.GetReservationsAsync(null, date);
            return Ok(result);
        }

        [HttpPut("{resKy}/approval")]
        public async Task<IActionResult> ApproveReservation(int resKy, [FromQuery] bool approve)
        {
             var result = await _service.ApproveReservationAsync(resKy, approve);
             return Ok(result.message);
        }
    }
}
