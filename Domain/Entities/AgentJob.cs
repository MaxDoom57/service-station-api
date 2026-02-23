namespace Domain.Entities
{
    public class AgentJob
    {
        public Guid JobId { get; set; }
        public int CKy { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string Status { get; set; } = "PENDING"; // PENDING | PROCESSING | COMPLETED | FAILED
        public string? ResultJson { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PickedUpAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(2);
    }
}
