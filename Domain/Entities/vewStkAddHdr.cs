using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class vewStkAddHdr
    {
        [MaxLength(15)]
        public string? LocCd { get; set; }  

        public DateTime? TrnDt { get; set; }  

        [MaxLength(60)]
        public string? Des { get; set; }   

        public short CKy { get; set; }    

        public int TrnKy { get; set; }   

        public int TrnNo { get; set; }  

        public short LocKy { get; set; }   
    }
}
