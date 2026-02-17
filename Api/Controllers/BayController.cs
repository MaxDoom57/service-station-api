using Application.DTOs.Bay;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/bay")]
    [Authorize]
    public class BayController : ControllerBase
    {
        private readonly BayService _service;

        public BayController(BayService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetBays()
        {
            try
            {
                var result = await _service.GetActiveBaysAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddBay([FromBody] CreateBayDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid details");

            try
            {
                var result = await _service.AddBayAsync(dto);
                if (!result.success) return BadRequest(result.message);
                
                return CreatedAtAction(nameof(GetBays), new { message = result.message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBay([FromBody] UpdateBayDto dto)
        {
             if (!ModelState.IsValid) return BadRequest("Invalid details");
 
             try
             {
                var result = await _service.UpdateBayAsync(dto);
                if (!result.success) return BadRequest(result.message);
    
                return Ok(result.message);
             }
             catch (Exception ex)
             {
                 return BadRequest(ex.Message);
             }
        }

        [HttpDelete("{bayKy}")]
        public async Task<IActionResult> DeleteBay(int bayKy)
        {
            try
            {
                var result = await _service.DeleteBayAsync(bayKy);
                if (!result.success) return BadRequest(result.message);
    
                return Ok(result.message);
            }
            catch (Exception ex)
            {
                 return BadRequest(ex.Message);
            }
        }
    }
}
