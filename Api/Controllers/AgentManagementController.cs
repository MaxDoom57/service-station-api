using Infrastructure.Context;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/agent-management")]
    [Authorize(Roles = "SuperAdmin")]
    public class AgentManagementController : ControllerBase
    {
        private readonly AgentTokenService _tokenService;
        private readonly MainDbContext _cloudDb;

        public AgentManagementController(AgentTokenService tokenService, MainDbContext cloudDb)
        {
            _tokenService = tokenService;
            _cloudDb      = cloudDb;
        }

        /// <summary>
        /// Call once per client company to generate their long-lived agent JWT.
        /// Store the returned token in the agent's appsettings.json under Agent:AgentToken.
        /// </summary>
        [HttpPost("generate-token/{companyKey:int}")]
        public IActionResult GenerateAgentToken(int companyKey)
        {
            var token = _tokenService.GenerateAgentToken(companyKey);
            return Ok(new
            {
                companyKey,
                agentToken = token,
                note = "Put this token in the agent's appsettings.json under Agent:AgentToken"
            });
        }

        /// <summary>
        /// Check whether the agent for a given company is currently alive
        /// (i.e. picked up a job within the last 10 seconds).
        /// </summary>
        [HttpGet("status/{companyKey:int}")]
        public async Task<IActionResult> GetAgentStatus(int companyKey)
        {
            var lastJob = await _cloudDb.AgentJobs
                .Where(j => j.CKy == companyKey && j.PickedUpAt != null)
                .OrderByDescending(j => j.PickedUpAt)
                .FirstOrDefaultAsync();

            if (lastJob == null)
                return Ok(new { status = "UNKNOWN", message = "No activity recorded yet" });

            var secondsAgo = (DateTime.UtcNow - lastJob.PickedUpAt!.Value).TotalSeconds;
            var isAlive    = secondsAgo < 10;

            return Ok(new
            {
                status      = isAlive ? "ONLINE" : "OFFLINE",
                lastSeenAgo = $"{(int)secondsAgo} seconds ago",
                lastJobId   = lastJob.JobId
            });
        }
    }
}
