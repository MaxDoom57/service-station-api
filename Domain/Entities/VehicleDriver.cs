using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class VehicleDriver
    {
        [Key]
        public int VehicleDriverKy { get; set; }
        public int VehicleKy { get; set; }
        public int DriverKy { get; set; }
    }
}
