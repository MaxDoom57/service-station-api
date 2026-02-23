using Application.DTOs.Agent;
using Domain.Common;
using Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/agent")]
    [Authorize]
    public class AgentBridgeController : ControllerBase
    {
        private readonly MainDbContext _cloudDb;
        private readonly ILogger<AgentBridgeController> _logger;

        public AgentBridgeController(MainDbContext cloudDb, ILogger<AgentBridgeController> logger)
        {
            _cloudDb = cloudDb;
            _logger  = logger;
        }

        /// <summary>
        /// Agent polls this endpoint to retrieve its pending jobs.
        /// </summary>
        [HttpGet("jobs/{companyKey:int}")]
        public async Task<IActionResult> GetPendingJobs(int companyKey)
        {
            // Security: agent token's CKy claim must match the requested companyKey
            var tokenCKy = int.Parse(User.FindFirst("cky")!.Value);
            if (tokenCKy != companyKey)
                return Forbid();

            var jobs = await _cloudDb.AgentJobs
                .Where(j => j.CKy == companyKey
                         && j.Status == AgentJobStatus.Pending
                         && j.ExpiresAt > DateTime.UtcNow)
                .OrderBy(j => j.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var job in jobs)
            {
                job.Status     = AgentJobStatus.Processing;
                job.PickedUpAt = DateTime.UtcNow;
            }

            await _cloudDb.SaveChangesAsync();

            var dtos = jobs.Select(j => new AgentJobDto
            {
                JobId   = j.JobId,
                CKy     = j.CKy,
                JobType = j.JobType,
                Payload = j.Payload,
                Status  = j.Status
            });

            return Ok(dtos);
        }

        /// <summary>
        /// Agent posts the execution result back to the API.
        /// </summary>
        [HttpPost("jobs/{jobId:guid}/result")]
        public async Task<IActionResult> PostJobResult(Guid jobId, [FromBody] JobResultDto dto)
        {
            var job = await _cloudDb.AgentJobs
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null) return NotFound();

            job.Status       = dto.Success ? AgentJobStatus.Completed : AgentJobStatus.Failed;
            job.ResultJson   = dto.ResultJson;
            job.ErrorMessage = dto.ErrorMessage;
            job.CompletedAt  = DateTime.UtcNow;

            await _cloudDb.SaveChangesAsync();

            _logger.LogInformation("Job {JobId} completed. Success: {Success}", jobId, dto.Success);

            return Ok();
        }
    }
}
