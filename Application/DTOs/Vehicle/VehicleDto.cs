namespace Application.DTOs.Vehicle
{
    public class VehicleDto
    {
        public int VehicleKy { get; set; }
        public string VehicleId { get; set; }
        public int OwnerAccountKy { get; set; }
        public string? FuelTyp { get; set; }
        public float? CurrentMileage { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? ChassisNo { get; set; }
        public string? EngineNo { get; set; }
        public string? Description { get; set; }
    }
}
