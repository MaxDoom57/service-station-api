using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// AccAdr class.
    /// </summary>
    public class AccAdr
    {
        public int AccKy { get; set; }
        public int AdrKy { get; set; }
        [NotMapped]
        public string? A { get; set; }   // char(10)
    }
}
