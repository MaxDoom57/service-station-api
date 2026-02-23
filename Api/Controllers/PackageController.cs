
using Application.DTOs.Package;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/package")]
    [Authorize]
    public class PackageController : ControllerBase
    {
        private readonly PackageService _service;
        private readonly ILogger<PackageController> _logger;  // ← added

        public PackageController(PackageService service, ILogger<PackageController> logger)  // ← added
        {
            _service = service;
            _logger = logger;  // ← added
        }

        [HttpGet]
        public async Task<IActionResult> GetPackages()
        {
            var methodName = nameof(GetPackages);
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("[{Method}] Started at {Time}", methodName, DateTime.UtcNow);

            try
            {
                _logger.LogInformation("[{Method}] Calling PackageService.GetPackagesAsync...", methodName);
                var result = await _service.GetPackagesAsync();

                stopwatch.Stop();
                _logger.LogInformation("[{Method}] Completed in {Ms}ms. Packages returned: {Count}",
                    methodName, stopwatch.ElapsedMilliseconds, result?.Count ?? 0);

                return Ok(result);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError("[{Method}] Failed after {Ms}ms", methodName, stopwatch.ElapsedMilliseconds);
                _logger.LogError("[{Method}] Exception Type : {Type}", methodName, ex.GetType().Name);
                _logger.LogError("[{Method}] Message        : {Message}", methodName, ex.Message);

                var inner = ex.InnerException;
                int depth = 1;
                while (inner != null)
                {
                    _logger.LogError("[{Method}] InnerException[{Depth}] {Type}: {Message}",
                        methodName, depth, inner.GetType().Name, inner.Message);
                    inner = inner.InnerException;
                    depth++;
                }

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