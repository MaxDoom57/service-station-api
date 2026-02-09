namespace Application.DTOs.Vehicle
{
    public class VehicleDetailDto
    {
        public int VehicleKy { get; set; }
        public string VehicleId { get; set; }
        public int VehicleTypKy { get; set; }
        public string VehicleTyp { get; set; } // Derived from CdMas logic if needed, or just ID
        public string? FuelTyp { get; set; }
        public float? CurrentMileage { get; set; }
        public DateTime? MileageUpdateDtm { get; set; }
        public float? FuelLevel { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? ChassisNo { get; set; }
        public string? EngineNo { get; set; }
        public string? Description { get; set; }

        public OwnerDto Owner { get; set; }
        public List<DriverDto> Drivers { get; set; }
    }
}
