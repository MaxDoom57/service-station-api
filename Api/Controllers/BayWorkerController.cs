using Application.DTOs.BayWorker;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/bayworker")]
    [Authorize]
    public class BayWorkerController : ControllerBase
    {
        private readonly BayWorkerService _service;

        public BayWorkerController(BayWorkerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWorkers()
        {
            var workers = await _service.GetAllWorkersAsync();
            return Ok(workers);
        }

        [HttpPost]
        public async Task<IActionResult> AddWorker([FromBody] CreateBayWorkerDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid details");

            var result = await _service.AddWorkerToBayAsync(dto);
            if (!result.success) return BadRequest(result.message);

            return CreatedAtAction(nameof(GetAllWorkers), new { message = result.message, id = result.workerKy });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateWorker([FromBody] UpdateBayWorkerDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid details");

            var result = await _service.UpdateWorkerInBayAsync(dto);
            if (!result.success) return BadRequest(result.message);

            return Ok(result.message);
        }

        [HttpDelete("{bayWorkerKy}")]
        public async Task<IActionResult> RemoveWorker(int bayWorkerKy)
        {
            var result = await _service.DeleteWorkerFromBayAsync(bayWorkerKy);
            if (!result.success) return BadRequest(result.message);

            return Ok(result.message);
        }
    }
}
