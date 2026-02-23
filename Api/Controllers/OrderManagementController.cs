using Application.DTOs.Order;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/order")]
    [Authorize]
    public class OrderManagementController : ControllerBase
    {
        private readonly OrderManagementService _service;

        public OrderManagementController(OrderManagementService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _service.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get order by Order Number
        /// </summary>
        [HttpGet("orderno/{ordNo}")]
        public async Task<IActionResult> GetOrderByOrderNo(int ordNo)
        {
            try
            {
                var order = await _service.GetOrderByOrderNoAsync(ordNo);
                if (order == null)
                    return NotFound("Order not found");
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get order by Order Key
        /// </summary>
        [HttpGet("{ordKy}")]
        public async Task<IActionResult> GetOrderByKey(int ordKy)
        {
            try
            {
                var order = await _service.GetOrderByKeyAsync(ordKy);
                if (order == null)
                    return NotFound("Order not found");
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Create new order
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var result = await _service.CreateOrderAsync(dto);
                if (!result.success)
                    return BadRequest(result.message);
    
                return Ok(new { result.message, result.ordKy });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update existing order
        /// </summary>
        [HttpPut("{ordKy}")]
        public async Task<IActionResult> UpdateOrder(int ordKy, [FromBody] CreateOrderDto dto)
        {
            try
            {
                var updateDto = new UpdateOrderDto
                {
                    OrdKy = ordKy,
                    LocKy = dto.LocKy,
                    OrdTyp = dto.OrdTyp,
                    OrdTypKy = dto.OrdTypKy,
                    Adrky = dto.Adrky,
                    AccKy = dto.AccKy,
                    PmtTrmKy = dto.PmtTrmKy,
                    SlsPri = dto.SlsPri,
                    DisPer = dto.DisPer,
                    Des = dto.Des,
                    DocNo = dto.DocNo,
                    YurRef = dto.YurRef,
                    OrdDt = dto.OrdDt,
                    DlryDt = dto.DlryDt,
                    OrdFrqKy = dto.OrdFrqKy,
                    OrdStsKy = dto.OrdStsKy,
                    RepAdrKy = dto.RepAdrKy,
                    DistAdrKy = dto.DistAdrKy,
                    BUKy = dto.BUKy,
                    OrdCat1Ky = dto.OrdCat1Ky,
                    OrdCat2Ky = dto.OrdCat2Ky,
                    OrdCat3Ky = dto.OrdCat3Ky,
                    Amt1 = dto.Amt1,
                    Amt2 = dto.Amt2,
                    MarPer = dto.MarPer,
                    Status = dto.Status,
                    SKy = dto.SKy,
                    OrdRem = dto.OrdRem,
                    OrderDetails = dto.OrderDetails
                };
    
                var result = await _service.UpdateOrderAsync(updateDto);
                if (!result.success)
                    return BadRequest(result.message);
    
                return Ok(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete order (soft delete)
        /// </summary>
        [HttpDelete("{ordKy}")]
        public async Task<IActionResult> DeleteOrder(int ordKy)
        {
            try
            {
                var result = await _service.DeleteOrderAsync(ordKy);
                if (!result.success)
                    return BadRequest(result.message);
    
                return Ok(result.message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
