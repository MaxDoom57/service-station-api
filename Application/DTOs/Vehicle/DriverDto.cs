using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Vehicle
{
    public class DriverDto
    {
        public int? DriverKy { get; set; } // Optional for updates if new driver
        [MaxLength(60)]
        public string DriverName { get; set; }
        [MaxLength(20)]
        public string? NIC { get; set; }
        [MaxLength(30)]
        public string? TP { get; set; }
        [MaxLength(30)]
        public string? LicenseNo { get; set; }
    }
}
