using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Units
    {
        [Key]
        public short UnitKy { get; set; }                 // smallint, not nullable

        [MaxLength(15)]
        public string? Unit { get; set; }                 // varchar(15)

        public bool fInAct { get; set; }                  // bit, not nullable

        [MaxLength(15)]
        public string? UnitTyp { get; set; }              // varchar(15)

        public double ConvRate { get; set; }              // float, NOT NULL

        public bool? fDefault { get; set; }               // bit, nullable

        public bool fBase { get; set; }                   // bit, NOT NULL

        [MaxLength(60)]
        public string? Des { get; set; }                  // varchar(60)

        [MaxLength(2)]
        public string? Status { get; set; }               // char(2)

        public short SKy { get; set; }                    // smallint, not nullable

        public bool fSI { get; set; }                     // bit, not nullable

        public bool fImp { get; set; }                    // bit, not nullable

        public int? EntUsrKy { get; set; }                // int, nullable

        public DateTime? EntDtm { get; set; }             // smalldatetime, nullable

        public short? RelUnitKy { get; set; }             // smallint, nullable
    }
}
