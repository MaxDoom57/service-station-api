using Application.DTOs.Codes;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/code")]
    [Authorize]
    public class CodesController : ControllerBase
    {
        private readonly CodeService _service;

        public CodesController(CodeService service)
        {
            _service = service;
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetUserAccessibleControls()
        {
            try
            {
                var result = await _service.GetCodeTypesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCodesByType([FromBody] GetCodesByTypeRequestDto request)
        {
            if (request.ConCd == null)
                return BadRequest("Valid ConCd is required");

            try
            {
                var result = await _service.GetCodesByTypeAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCodeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest("Code is required");

            if (string.IsNullOrWhiteSpace(dto.ConCd))
                return BadRequest("ConCd is required");

            try
            {
                return Ok(await _service.CreateCodeAsync(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("{cdKy:int}")]
        public async Task<IActionResult> Update(int cdKy, [FromBody] UpdateCodeDto dto)
        {
            if (cdKy <= 0)
                return BadRequest("Invalid CdKy");

            if (string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest("Code is required");

            try
            {
                return Ok(await _service.UpdateCodeAsync(cdKy, dto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{cdKy}")]
        public async Task<IActionResult> DeleteCode(int cdKy)
        {
            if (cdKy <= 0)
                return BadRequest("Valid CdKy is required");

            try
            {
                var result = await _service.DeleteCodeAsync(cdKy);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
