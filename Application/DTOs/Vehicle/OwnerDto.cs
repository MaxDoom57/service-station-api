using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Vehicle
{
    public class OwnerDto
    {
        // Address Details
        public int? AdrKy { get; set; }
        [Required]
        [MaxLength(60)]
        public string FstNm { get; set; }
        [MaxLength(60)]
        public string? LstNm { get; set; }
        [MaxLength(30)]
        public string? TP1 { get; set; }
        [MaxLength(30)]
        public string? NIC { get; set; }
        [MaxLength(120)]
        public string? Address { get; set; }

        // Account Details (Optional if reusing existing account logic implicitly, but user said create account)
        // We will assume basic details for Account Creation are derived or minimal
    }
}
