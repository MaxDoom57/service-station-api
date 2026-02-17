using Application.DTOs.Package;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/package")]
    [Authorize]
    public class PackageController : ControllerBase
    {
        private readonly PackageService _service;

        public PackageController(PackageService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetPackages()
        {
            try
            {
                var result = await _service.GetPackagesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPackage([FromBody] CreatePackageDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid details");

            try
            {
                var result = await _service.AddPackageAsync(dto);
                if (!result.success) return BadRequest(result.message);
                
                return CreatedAtAction(nameof(GetPackages), new { message = result.message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePackage([FromBody] CreatePackageDto dto)
        {
             if (!ModelState.IsValid) return BadRequest("Invalid details");

            try
            {
                var result = await _service.UpdatePackageAsync(dto);
                if (!result.success) return BadRequest(result.message);

                return Ok(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{cdKy}")]
        public async Task<IActionResult> DeletePackage(int cdKy)
        {
            try
            {
                var result = await _service.DeletePackageAsync(cdKy);
                if (!result.success) return BadRequest(result.message);

                return Ok(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{cdKy}")]
        public async Task<IActionResult> GetPackageDetails(int cdKy)
        {
            try
            {
                var result = await _service.GetPackageDetailsAsync(cdKy);
                if (result == null) return NotFound("Package not found");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("with-items")]
        public async Task<IActionResult> GetAllPackagesWithItems()
        {
            try
            {
                var result = await _service.GetAllPackagesWithItemsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
