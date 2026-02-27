using Application.DTOs.VehicleType;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/vehicletype")]
    [Authorize]
    public class VehicleTypeController : ControllerBase
    {
        private readonly VehicleTypeService _service;

        public VehicleTypeController(VehicleTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleTypes()
        {
            try
            {
                var result = await _service.GetVehicleTypesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicleType([FromBody] CreateVehicleTypeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid details");

            var result = await _service.AddVehicleTypeAsync(dto);
            if (!result.success) return BadRequest(result.message);
            
            return CreatedAtAction(nameof(GetVehicleTypes), new { message = result.message });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateVehicleType([FromBody] CreateVehicleTypeDto dto)
        {
             if (!ModelState.IsValid) return BadRequest("Invalid details");

            var result = await _service.UpdateVehicleTypeAsync(dto);
            if (!result.success) return BadRequest(result.message);

            return Ok(result.message);
        }

        [HttpDelete("{cdKy}")]
        public async Task<IActionResult> DeleteVehicleType(int cdKy)
        {
            var result = await _service.DeleteVehicleTypeAsync(cdKy);
            if (!result.success) return BadRequest(result.message);

            return Ok(result.message);
        }
    }
}
