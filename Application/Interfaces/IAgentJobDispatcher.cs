using Application.DTOs.Agent;

namespace Application.Interfaces
{
    public interface IAgentJobDispatcher
    {
        Task<AgentJobResult> DispatchAndWaitAsync(
            int companyKey,
            string jobType,
            object payload,
            int timeoutSeconds = 15);
    }
}
