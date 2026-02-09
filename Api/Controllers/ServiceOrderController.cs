using Application.DTOs.ServiceOrder;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/serviceorder")]
    [Authorize]
    public class ServiceOrderController : ControllerBase
    {
        private readonly ServiceOrderService _service;

        public ServiceOrderController(ServiceOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceOrder([FromBody] CreateServiceOrderDto dto)
        {
            var result = await _service.CreateServiceOrderAsync(dto);
            if (!result.success) return BadRequest(result.message);
            return Ok(new { result.message, result.ordKy });
        }

        [HttpPost("item")]
        public async Task<IActionResult> AddServiceItem([FromBody] AddServiceItemDto dto)
        {
            var result = await _service.AddServiceItemAsync(dto);
            return result.success ? Ok(result.message) : BadRequest(result.message);
        }

        [HttpPut("item/approval")]
        public async Task<IActionResult> ApproveServiceItem([FromBody] ApproveServiceItemDto dto)
        {
            var result = await _service.ApproveServiceItemAsync(dto);
            return result.success ? Ok(result.message) : BadRequest(result.message);
        }

        [HttpPut("item/status")]
        public async Task<IActionResult> UpdateItemStatus([FromBody] UpdateItemStatusDto dto)
        {
            var result = await _service.UpdateItemStatusAsync(dto);
            return result.success ? Ok(result.message) : BadRequest(result.message);
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceOrders()
        {
            return Ok(await _service.GetServiceOrdersAsync());
        }

        [HttpGet("{ordKy}")]
        public async Task<IActionResult> GetServiceOrderDetails(int ordKy)
        {
            var result = await _service.GetServiceOrderDetailsAsync(ordKy);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
