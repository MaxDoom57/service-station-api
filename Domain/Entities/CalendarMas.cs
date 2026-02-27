using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("CalendarMas")]
    public class CalendarMas
    {
        [Key]
        public int CalKy { get; set; }
        public short CKy { get; set; }
        public DateTime? CalDt { get; set; }
        public bool fInAct { get; set; }
        public byte fApr { get; set; }
        public short? DayTypKy { get; set; }
        public int? WrkDay { get; set; }
        public string? CalDesc { get; set; }
        public short SKy { get; set; }
        public int? EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }
        public string? Status { get; set; }
    }
}
