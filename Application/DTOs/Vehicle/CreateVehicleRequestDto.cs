using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Vehicle
{
    public class CreateVehicleRequestDto
    {
        // Vehicle Details
        public int? VehicleKy { get; set; } // For Update
        [Required]
        [MaxLength(20)]
        public string VehicleId { get; set; }
        public int VehicleTypKy { get; set; }
        public string? FuelTyp { get; set; }
        public float? CurrentMileage { get; set; }
        public float? FuelLevel { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? ChassisNo { get; set; }
        public string? EngineNo { get; set; }
        public string? Description { get; set; }

        // Owner Details
        [Required]
        public OwnerDto Owner { get; set; }

        // Driver Details (One or Many)
        public List<DriverDto> Drivers { get; set; } = new List<DriverDto>();
    }
}
