using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/[controller]")]
    [Authorize]
    public class PaymentTermController : ControllerBase
    {
        private readonly PaymentTermService _service;

        public PaymentTermController(PaymentTermService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var terms = await _service.GetAllAsync();
            return Ok(terms);
        }
    }
}
