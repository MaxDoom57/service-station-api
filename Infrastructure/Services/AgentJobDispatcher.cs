using Application.DTOs.Agent;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class AgentJobDispatcher : IAgentJobDispatcher
    {
        private readonly MainDbContext _cloudDb;
        private readonly ILogger<AgentJobDispatcher> _logger;

        public AgentJobDispatcher(MainDbContext cloudDb, ILogger<AgentJobDispatcher> logger)
        {
            _cloudDb = cloudDb;
            _logger = logger;
        }

        public async Task<AgentJobResult> DispatchAndWaitAsync(
            int companyKey,
            string jobType,
            object payload,
            int timeoutSeconds = 15)
        {
            var job = new AgentJob
            {
                JobId     = Guid.NewGuid(),
                CKy       = companyKey,
                JobType   = jobType,
                Payload   = JsonSerializer.Serialize(payload),
                Status    = AgentJobStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(timeoutSeconds + 5)
            };

            _cloudDb.AgentJobs.Add(job);
            await _cloudDb.SaveChangesAsync();

            _logger.LogInformation("Job {JobId} dispatched. Type: {Type} CKy: {CKy}",
                job.JobId, jobType, companyKey);

            var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

            while (DateTime.UtcNow < deadline)
            {
                await Task.Delay(300);

                var updated = await _cloudDb.AgentJobs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(j => j.JobId == job.JobId);

                if (updated == null) break;

                if (updated.Status == AgentJobStatus.Completed)
                    return new AgentJobResult { Success = true, ResultJson = updated.ResultJson };

                if (updated.Status == AgentJobStatus.Failed)
                    return new AgentJobResult { Success = false, Error = updated.ErrorMessage };
            }

            // Timeout — clean up
            var timedOut = await _cloudDb.AgentJobs.FirstOrDefaultAsync(j => j.JobId == job.JobId);
            if (timedOut != null)
            {
                timedOut.Status       = AgentJobStatus.Failed;
                timedOut.ErrorMessage = "Agent timeout - agent may be offline";
                await _cloudDb.SaveChangesAsync();
            }

            _logger.LogWarning("Job {JobId} timed out after {Seconds}s", job.JobId, timeoutSeconds);

            return new AgentJobResult
            {
                Success = false,
                Error   = "Request timed out. The local agent may be offline. Please try again."
            };
        }
    }
}
