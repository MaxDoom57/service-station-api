using Application.DTOs.Vehicle;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/vehicle")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly VehicleService _service;

        public VehicleController(VehicleService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterVehicle([FromBody] CreateVehicleRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid details");

            var result = await _service.RegisterVehicleAsync(dto);
            return StatusCode(result.statusCode, new { message = result.message });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateVehicle([FromBody] CreateVehicleRequestDto dto)
        {
            var result = await _service.UpdateVehicleAsync(dto);
            if (!result.success) return BadRequest(result.message);
            return Ok(result.message);
        }

        [HttpDelete("{vehicleKy}")]
        public async Task<IActionResult> DeleteVehicle(int vehicleKy)
        {
            var result = await _service.DeleteVehicleAsync(vehicleKy);
            if (!result.success) return BadRequest(result.message);
            return Ok(result.message);
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveVehicles()
        {
            try
            {
                var result = await _service.GetActiveVehiclesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetAllVehiclesDetailed()
        {
            try
            {
                var result = await _service.GetAllVehiclesDetailedAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{vehicleKy}")]
        public async Task<IActionResult> GetVehicleDetails(int vehicleKy)
        {
            try
            {
                var result = await _service.GetVehicleDetailsAsync(vehicleKy);
                if (result == null) return NotFound("Vehicle not found");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
