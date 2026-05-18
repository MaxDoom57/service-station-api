namespace Application.DTOs.Agent
{
    /// <summary>
    /// JobResultDto class.
    /// </summary>
    public class JobResultDto
    {
        public bool Success { get; set; }
        public string? ResultJson { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
