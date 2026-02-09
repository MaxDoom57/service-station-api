using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class vewItmBatch
    {
        public int ItmKy { get; set; }                  

        [MaxLength(25)]
        public string? ItmCd { get; set; }      

        [MaxLength(60)]
        public string? PartNo { get; set; }    

        [MaxLength(60)]
        public string? ItmNm { get; set; }     

        public int ItmBatchKy { get; set; }     

        [MaxLength(30)]
        public string? BatchNo { get; set; }   

        public DateTime? ExpirDt { get; set; }    

        [Column(TypeName = "real")]
        public float CosPri { get; set; }     

        [Column(TypeName = "real")]
        public float SalePri { get; set; }   

        [Column(TypeName = "real")]
        public float Qty { get; set; }    

        public double? ItmLocQty { get; set; } 
    }
}
