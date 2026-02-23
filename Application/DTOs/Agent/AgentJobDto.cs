namespace Application.DTOs.Agent
{
    public class AgentJobDto
    {
        public Guid JobId { get; set; }
        public int CKy { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
