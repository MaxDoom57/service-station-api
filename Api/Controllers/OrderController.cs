using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceStationApi.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly InvoiceService _service;

        public OrderController(InvoiceService service)
        {
            _service = service;
        }


        [HttpGet]
        public async Task<IActionResult> GetOrderDetailsAsync(int trnNo)
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
    }
}
