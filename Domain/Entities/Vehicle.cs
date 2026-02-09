using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Vehicle
    {
        [Key]
        public int VehicleKy { get; set; }

        [MaxLength(20)]
        public string VehicleId { get; set; } // Vehicle Number

        public int OwnerAccountKy { get; set; }

        public bool fInAct { get; set; }

        public int VehicleTypKy { get; set; }

        [MaxLength(20)]
        public string? FuelTyp { get; set; } // Petrol or Diesel

        [Column(TypeName = "real")]
        public float? CurrentMileage { get; set; }

        public DateTime? MileageUpdateDtm { get; set; }

        [Column(TypeName = "real")]
        public float? FuelLevel { get; set; }

        [MaxLength(30)]
        public string? Make { get; set; }

        [MaxLength(30)]
        public string? Model { get; set; }

        public int? Year { get; set; }

        [MaxLength(50)]
        public string? ChassisNo { get; set; }

        [MaxLength(50)]
        public string? EngineNo { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        public int? EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }
    }
}
