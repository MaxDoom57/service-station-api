using Application.DTOs.Vehicle;
using System;
using System.Collections.Generic;

namespace Application.DTOs.ServiceOrder
{
    public class CreateServiceOrderDto
    {
        // Customer Details (If new, simplified. If existing, we lookup by Vehicle usually, but here explicit details requested)
        public string CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        // If system needs to link to existing account, we might need Account ID, but prompt says "with customer details(name, id...)"
        // I'll assume we try to match or use existing Account from Vehicle if available.
        
        // Vehicle Details
        public string VehicleId { get; set; }
        public float? CurrentMileage { get; set; }
        public string? DamageNote { get; set; }
        public string? AdditionalNotes { get; set; } // Remarks
        
        // Service Details
        public int? PackageKy { get; set; }                              // Optional
        public List<ServiceOrderItemInputDto>? Items { get; set; }       // Used when no PackageKy
        public int BayKy { get; set; }
        public string? UserId { get; set; }
        
        // Image Details
        public string? SignatureImage { get; set; } // Base64
        public List<string>? VehicleImages { get; set; } // List of Base64
    }

    public class ServiceOrderItemInputDto
    {
        public int ItmKy { get; set; }
        public string? EstimatedTime { get; set; } // Defaults to "Standard" if not set
    }

    public class AddServiceItemDto
    {
        public int ServiceOrdKy { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public string EstimatedTime { get; set; }
        public string? UserId { get; set; }
    }

    public class ApproveServiceItemDto
    {
        public int ServiceOrdDetKy { get; set; }
        public bool IsApproved { get; set; } // If false, maybe reject/delete?
        
        // Approval Details
        public string CustName { get; set; }
        public string IpAddress { get; set; }
        public string Device { get; set; }
    }

    public class UpdateItemStatusDto
    {
        public int ServiceOrdDetKy { get; set; }
        // "wait, inprogress, finish"
        public string Status { get; set; } // "Wait", "InProgress", "Finish"
        public string? UserId { get; set; }
    }

    public class UpdateServiceOrderStatusDto
    {
        public int ServiceOrdKy { get; set; }
        public string Status { get; set; } 
        public string? UserId { get; set; }
    }

    public class ServiceOrderDetailDto
    {
        public int ServiceOrdDetKy { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } // Derived from columns
        public bool IsApproved { get; set; }
    }

    public class ServiceOrderDto
    {
        public int ServiceOrdKy { get; set; }
        public string ServiceOrdNo { get; set; }
        public DateTime Date { get; set; }
        public string VehicleId { get; set; }
        public string CustomerName { get; set; }
        public string PackageName { get; set; }
        public string Status { get; set; }
        public List<ServiceOrderDetailDto> Items { get; set; }
    }

    public class ServiceOrderImageDto
    {
        public int ImageKy { get; set; }
        public string ImageUrl { get; set; }
        public string ImageType { get; set; }
    }
}
