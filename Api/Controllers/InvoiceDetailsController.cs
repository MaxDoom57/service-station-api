using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/[controller]")]
    [Authorize]
    public class InvoiceDetailsController : ControllerBase
    {
        private readonly InvoiceDetailsService _service;

        public InvoiceDetailsController(InvoiceDetailsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails()
        {
            var result = await _service.GetDetailsAsync();
            return Ok(result);
        }
    }
}
