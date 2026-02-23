using System.Text.Json;

namespace Application.DTOs.Agent
{
    public class AgentJobResult
    {
        public bool Success { get; set; }
        public string? ResultJson { get; set; }
        public string? Error { get; set; }

        public T? Deserialize<T>() =>
            ResultJson == null ? default
            : JsonSerializer.Deserialize<T>(ResultJson);
    }
}
