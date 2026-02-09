using Application.DTOs.ItemBatch;
using Application.DTOs.Items;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/items")]
    [Authorize]
    public class ItemsController : ControllerBase
    {
        private readonly ItemService _service;

        public ItemsController(ItemService service)
        {
            _service = service;
        }

        // GET api/ssms/v0.1/items
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var items = await _service.GetAllItemsAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetAllActiveItems()
        {
            try
            {
                var items = await _service.GetItemsWithoutFInActAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] AddItemDTO dto)
        {
            try
            {
                if (dto == null || !ModelState.IsValid)
                    return BadRequest(new { message = "Invalid item details" });

                var result = await _service.AddItemAsync(dto);

                if (!result.success)
                    return StatusCode(result.statusCode, new { message = result.message });

                return StatusCode(201, new { message = "Item added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateItemDTO dto)
        {
            try
            {
                var result = await _service.UpdateItemAsync(dto);

                return StatusCode(result.statusCode, new { message = result.message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{itemKey}")]
        public async Task<IActionResult> DeleteItem([FromRoute] int itemKey)
        {
            try
            {
                var result = await _service.DeleteItemAsync(itemKey);
                return StatusCode(result.statusCode, new { message = result.message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("batch/{itemKey}")]
        public async Task<IActionResult> GetItemBatches(int itemKey)
        {
            try
            {
                var batches = await _service.GetItemBatchesAsync(itemKey);

                if (batches == null || batches.Count == 0)
                    return NotFound(new { message = "No batches found" });

                return Ok(batches);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> AddItemBatch([FromBody] AddItemBatchDTO dto)
        {
            try
            {
                var result = await _service.AddItemBatchAsync(dto);

                return StatusCode(result.statusCode, new
                {
                    message = result.message,
                    batchKey = result.batchKey
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("batch")]
        public async Task<IActionResult> UpdateItemBatch([FromBody] UpdateItemBatchDTO dto)
        {
            try
            {
                var result = await _service.UpdateItemBatchAsync(dto);

                return StatusCode(result.statusCode, new
                {
                    message = result.message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
