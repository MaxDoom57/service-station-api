using Application.DTOs.Reservation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/reservation")]
    [AllowAnonymous]
    public class ReservationController : ControllerBase
    {
        private readonly ReservationService _service;
        private readonly OtpService _otpService;

        public ReservationController(ReservationService service, OtpService otpService)
        {
            _service = service;
            _otpService = otpService;
        }

        // ─── OTP Flow ────────────────────────────────────────────────────────────

        [HttpPost("otp/init")]
        public async Task<IActionResult> OtpInit([FromBody] CreateFullReservationDto dto)
        {
            try
            {
                var result = await _otpService.InitAsync(dto);
                if (!result.success) return BadRequest(new { result.message });
                return Ok(new { result.message, result.sessionId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("otp/confirm")]
        public async Task<IActionResult> OtpConfirm([FromBody] OtpConfirmDto dto)
        {
            try
            {
                var result = await _otpService.ConfirmAsync(dto);
                if (!result.success) return BadRequest(new { result.message });
                return Ok(new { result.message, result.resKy });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("otp/resend")]
        public async Task<IActionResult> OtpResend([FromBody] OtpResendDto dto)
        {
            try
            {
                var result = await _otpService.ResendAsync(dto);
                if (!result.success) return BadRequest(new { result.message });
                return Ok(new { result.message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
