namespace Application.DTOs.Vehicle
{
    public class VehicleOwnerDto
    {
        public int VehicleKy { get; set; }
        public required string VehicleId { get; set; } // Vehicle No
        public required string OwnerName { get; set; }
        public int OwnerAccountKy { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
    }
}
