using Application.DTOs.Lookups;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/lookups")]
    [Authorize]
    public class LookupsController : ControllerBase
    {
        private readonly LookupService _service;

        public LookupsController(LookupService service)
        {
            _service = service;
        }

        [HttpGet("itemCategory1")]
        public async Task<IActionResult> GetItemCategory1()
        {
            try
            {
                return Ok(await _service.GetItemCategory1Async());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("itemCategory2")]
        public async Task<IActionResult> GetItemCategory2()
        {
            try
            {
                return Ok(await _service.GetItemCategory2Async());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("itemCategory3")]
        public async Task<IActionResult> GetItemCategory3()
        {
            try
            {
                return Ok(await _service.GetItemCategory3Async());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("itemCategory4")]
        public async Task<IActionResult> GetItemCategory4()
        {
            try
            {
                return Ok(await _service.GetItemCategory4Async());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("trnNoLast")]
        public async Task<IActionResult> GetLastTransactionNo([FromBody] GetLastTrnNoRequestDto request)
        {
            try
            {
                var trnNo = await _service.GetLastTransactionNoAsync(request);

                return Ok(new
                {
                    OurCd = request.OurCd,
                    LastTransactionNo = trnNo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
