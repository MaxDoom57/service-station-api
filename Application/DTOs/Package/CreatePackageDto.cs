using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Package
{
    /// <summary>
    /// CreatePackageDto class.
    /// </summary>
    public class CreatePackageDto
    {
        public int? CdKy { get; set; } // For Update

        [Required]
        [MaxLength(15)]
        public string Code { get; set; }

        [Required]
        [MaxLength(60)]
        public string CdNm { get; set; }
    }
}
