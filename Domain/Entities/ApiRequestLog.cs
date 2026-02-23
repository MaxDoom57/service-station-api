using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ApiRequestLog")]
    public class ApiRequestLog
    {
        [Key]
        public long LogKy { get; set; }
        public string? RequestPath { get; set; }
        public string? Method { get; set; }
        public string? QueryString { get; set; }
        public string? RequestBody { get; set; }
        public string? DeviceDetails { get; set; } // User-Agent, IP, etc.
        public int ResponseStatusCode { get; set; }
        public string? ResponseBody { get; set; } // Optional, "what done"
        public DateTime RequestTime { get; set; }
        public long DurationMs { get; set; }
        public int? UserId { get; set; } 
        public int? CompanyKey { get; set; }
    }
}
