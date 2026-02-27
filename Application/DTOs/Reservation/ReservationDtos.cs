using Application.DTOs.Vehicle;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reservation
{
    public class CreateFullReservationDto
    {
        // 1. Vehicle Info
        // If VehicleId is provided and exists, we use it. 
        // If VehicleId is provided and NEW, we need Owner Details (so we include CreateVehicleRequestDto logic here or nested)
        
        public string VehicleId { get; set; } // Required linkage
        
        // If New Vehicle/User
        public CreateVehicleRequestDto? NewVehicleDetails { get; set; } 

        // 2. Reservation Info
        [Required]
        public int PackageKy { get; set; }
        
        [Required]
        public int BayKy { get; set; }

        [Required]
        public DateTime BookingFrom { get; set; }

        [Required]
        public DateTime BookingTo { get; set; }

        public string? Remarks { get; set; }
    }

    public class ReservationDetailDto
    {
        public int ResKy { get; set; }
        public int VehicleKy { get; set; }
        public string VehicleId { get; set; }
        public string VehicleType { get; set; }
        
        // Owner Info (Simplified)
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }

        // Package
        public int PackageKy { get; set; }
        public string PackageName { get; set; }

        // Time / Bay
        public int BayKy { get; set; }
        public string BayName { get; set; }
        public DateTime FromDtm { get; set; }
        public DateTime ToDtm { get; set; }
        
        public string ResStatus { get; set; }
        public string? Remarks { get; set; }
    }
}
