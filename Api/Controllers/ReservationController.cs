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
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
    
                var result = await _service.CreateReservationAsync(dto);
                if (!result.success) return BadRequest(result.message);
    
                return Ok(new { message = result.message, resKy = result.resKy });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{resKy}")]
        public async Task<IActionResult> UpdateReservation(int resKy, [FromBody] CreateFullReservationDto dto)
        {
            try
            {
                var result = await _service.UpdateReservationAsync(resKy, dto);
                if (!result.success) return BadRequest(result.message);
                return Ok(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{resKy}")]
        public async Task<IActionResult> DeleteReservation(int resKy)
        {
            try
            {
                var result = await _service.DeleteReservationAsync(resKy);
                if (!result.success) return BadRequest(result.message);
                return Ok(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            try
            {
                var result = await _service.GetReservationsAsync(null, null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetReservationsByVehicle(string vehicleId)
        {
            try
            {
                var result = await _service.GetReservationsAsync(vehicleId, null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetReservationsByDate(DateTime date)
        {
            try
            {
                var result = await _service.GetReservationsAsync(null, date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{resKy}/approval")]
        public async Task<IActionResult> ApproveReservation(int resKy, [FromQuery] bool approve)
        {
             try
             {
                 var result = await _service.ApproveReservationAsync(resKy, approve);
                 return Ok(result.message);
             }
             catch (Exception ex)
             {
                 return BadRequest(ex.Message);
             }
        }
    }
}
