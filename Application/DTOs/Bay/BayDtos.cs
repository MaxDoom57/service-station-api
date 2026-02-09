using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Bay
{
    public class BayDto
    {
        public int BayKy { get; set; }
        public string BayCd { get; set; }
        public string BayNm { get; set; }
        public bool IsReservationAvailable { get; set; }
        public string? Description { get; set; }
    }

    public class CreateBayDto
    {
        [Required]
        [MaxLength(20)]
        public string BayCd { get; set; }

        [Required]
        [MaxLength(60)]
        public string BayNm { get; set; }

        public bool IsReservationAvailable { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateBayDto
    {
        [Required]
        public int BayKy { get; set; }

        [Required]
        [MaxLength(20)]
        public string BayCd { get; set; }

        [Required]
        [MaxLength(60)]
        public string BayNm { get; set; }

        public bool IsReservationAvailable { get; set; }
        public string? Description { get; set; }
    }
}
