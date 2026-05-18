namespace Application.DTOs.Services
{
    /// <summary>
    /// CreateServiceDto class.
    /// </summary>
    public class CreateServiceDto
    {
        public int CKy { get; set; }
        public required string VehicleId { get; set; }
        public required string ServiceName { get; set; } // Maps to ServiceNm
        public bool fFinish { get; set; }
        public bool fTerminate { get; set; }
    }
}
