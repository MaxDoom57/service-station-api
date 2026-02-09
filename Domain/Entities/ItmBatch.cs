using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ItmBatch
    {
        [Key]
        public int ItmBatchKy { get; set; } 

        public int ItmKy { get; set; } 

        [MaxLength(30)]
        public string? BatchNo { get; set; } 

        public DateTime? ExpirDt { get; set; } 

        [Column(TypeName = "real")]
        public float? CosPri { get; set; } 

        [Column(TypeName = "real")]
        public float? SalePri { get; set; } 

        [Column(TypeName = "real")]
        public float? Qty { get; set; } 
    }
}
