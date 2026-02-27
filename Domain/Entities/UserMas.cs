using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("UsrMas")]
    public class UsrMas
    {
        [Key]
        public int UsrKy { get; set; }

        [Required]
        public short CKy { get; set; }

        [StringLength(30)]
        public string? UsrId { get; set; }

        public bool? fInAct { get; set; }

        public byte? flApr { get; set; }

        [Required]
        public byte fGroup { get; set; }

        [Required]
        public bool fSysAccNm { get; set; }

        [StringLength(60)]
        public string? UsrNm { get; set; }

        public int? BUKy { get; set; }

        [StringLength(60)]
        public string? PwdTip { get; set; }

        public short? AcsLvlKy { get; set; }

        [StringLength(30)]
        public string? PID { get; set; }

        [StringLength(30)]
        public string? Pwd { get; set; }

        [StringLength(2)]
        [Column(TypeName = "char(2)")]
        public string? Status { get; set; }

        [Required]
        public short SKy { get; set; }

        public int? EntUsrKy { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? EntDtm { get; set; }

        //[StringLength(255)]
        //public string? Trace { get; set; }
    }
}
