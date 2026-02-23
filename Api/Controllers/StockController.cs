using Application.DTOs.Stock_Addition;
using Application.DTOs.StockDeduction;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/[controller]")]
    [Authorize]
    public class StockController : ControllerBase
    {
        private readonly StockService _service;

        public StockController(StockService service)
        {
            _service = service;
        }

        [HttpGet("addition/{trnNo}")]
        public async Task<IActionResult> GetStockAddition(int trnNo)
        {
            var result = await _service.GetStockAdditionByTrnNoAsync(trnNo);

            return StatusCode(result.statusCode, new
            {
                data = result.data
            });
        }

        [HttpPost("addition")]
        public async Task<IActionResult> CreateStockAddition([FromBody] StockAddPostDTO dto)
        {
            var result = await _service.CreateStockAdditionAsync(dto);

            return StatusCode(result.statusCode, new
            {
                message = result.message,
                trnNo = result.trnNo,
                trnKy = result.trnKy
            });
        }

        [HttpPut("addition")]
        public async Task<IActionResult> UpdateStockAddition([FromBody] StockAddUpdateDTO dto)
        {
            var result = await _service.UpdateStockAdditionAsync(dto);

            return StatusCode(result.statusCode, new
            {
                message = result.message,
                trnNo = result.trnNo,
                trnKy = result.trnKy
            });
        }

        [HttpDelete("addition/{trnNo}")]
        public async Task<IActionResult> DeleteStockAddition([FromRoute] int trnNo)
        {
            var result = await _service.DeleteStockAdditionAsync(trnNo);

            return StatusCode(result.statusCode, new
            {
                message = result.message
            });
        }


        [HttpGet("deduction/{trnNo}")]
        public async Task<IActionResult> GetStockDeduction(int trnNo)
        {
            var result = await _service.GetStockDeductionByTrnNoAsync(trnNo);

            return StatusCode(result.statusCode, new
            {
                data = result.data
            });
        }

        [HttpPost("deduction")]
        public async Task<IActionResult> CreateStockDeduction([FromBody] StockDeductionPostDTO dto)
        {
            var result = await _service.CreateStockDeductionAsync(dto);

            return StatusCode(result.statusCode, new
            {
                message = result.message,
                trnNo = result.trnNo,
                trnKy = result.trnKy
            });
        }

        [HttpPut("deduction")]
        public async Task<IActionResult> UpdateStockDeduction([FromBody] StockDeductionUpdateDTO dto)
        {
            var result = await _service.UpdateStockDeductionAsync(dto);

            return StatusCode(result.statusCode, new
            {
                message = result.message,
                trnNo = result.trnNo,
                trnKy = result.trnKy
            });
        }

        [HttpDelete("deduction/{trnNo}")]
        public async Task<IActionResult> DeleteStockDeduction([FromRoute] int trnNo)
        {
            var result = await _service.DeleteStockDeductionAsync(trnNo);

            return StatusCode(result.statusCode, new
            {
                message = result.message
            });
        }
    }
}
