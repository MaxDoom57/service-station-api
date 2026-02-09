using Application.DTOs.Invoice;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/[controller]")]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceService _service;

        public InvoiceController(InvoiceService service)
        {
            _service = service;
        }


        [HttpGet("{trnNo}")]
        public async Task<IActionResult> GetInvoiceByTrnNo(int trnNo)
        {
            try
            {
                if (trnNo <= 0)
                    return BadRequest("Valid TrnNo is required");

                var result = await _service.GetInvoiceByTrnKyAsync(trnNo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddInvoice([FromBody] InvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid invoice data" });

            try
            {
                int trnKy = await _service.AddInvoiceAsync(dto);
                return StatusCode(201, new
                {
                    message = "Invoice added successfully",
                    TrnKy = trnKy
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Invoice saving failed",
                    error = ex.Message
                });
            }
        }


        [HttpPut("{trnKy}")]
        public async Task<IActionResult> UpdateInvoice(int trnKy, [FromBody] UpdateInvoiceDto dto)
        {
            if (dto == null || trnKy != dto.TrnKy)
                return BadRequest(new { message = "Invalid invoice data" });

            try
            {
                var ok = await _service.UpdateInvoiceAsync(dto);
                return Ok(new
                {
                    message = "Invoice updated successfully",
                    TrnKy = trnKy
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Invoice update failed",
                    error = ex.Message
                });
            }
        }
    }
}
