namespace Application.DTOs.Agent
{
    public class JobResultDto
    {
        public bool Success { get; set; }
        public string? ResultJson { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
