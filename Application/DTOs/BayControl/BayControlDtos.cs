using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.BayControl
{
    public class AvailableBayDto
    {
        public int BayKy { get; set; }
        public string BayCd { get; set; }
        public string BayNm { get; set; }
        public string? Status { get; set; } // "Available"
        public string? CurrentActivity { get; set; }
    }

    public class BayStatusDto
    {
        public int BayKy { get; set; }
        public string BayCd { get; set; }
        public string BayNm { get; set; }
        public bool IsOccupied { get; set; }
        public int? CurrentVehicleKy { get; set; }
        public string? VehicleNumber { get; set; } // If needed, joined
        public string? CurrentActivity { get; set; }
        public DateTime? EstimatedFinishDtm { get; set; }
    }

    public class CreateReservationDto
    {
        public int? VehicleKy { get; set; } // Optional if reserving bay without vehicle yet? No usually has vehicle.
        public int BayKy { get; set; }
        public DateTime FromDtm { get; set; }
        public DateTime ToDtm { get; set; }
        public string ResType { get; set; } // Online, Physical
    }

    public class ReservationDto
    {
        public int ResKy { get; set; }
        public int BayKy { get; set; }
        public string BayNm { get; set; }
        public int? VehicleKy { get; set; }
        
        // Let's add VehicleId if possible, but basic DTO first
        public DateTime FromDtm { get; set; }
        public DateTime ToDtm { get; set; }
        public string ResType { get; set; }
        public string ResStatus { get; set; }
    }

    public class UpdateBayControlDto
    {
        public int BayKy { get; set; }
        public bool IsOccupied { get; set; }
        public int? VehicleKy { get; set; }
        public string? CurrentActivity { get; set; }
        public DateTime? EstimatedFinishDtm { get; set; }
    }
}
