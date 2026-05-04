using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class SvschkList
    {
        [Key]
        public int ChkListKy { get; set; }
        public int? OrdKy { get; set; }
        public int? CdKy { get; set; }
        public bool? bitValue1 { get; set; }
        public bool? bitValue2 { get; set; }
        public float? Val1 { get; set; }
        [MaxLength(60)]
        public string? Remarks { get; set; }
    }
}
