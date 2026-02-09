using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Driver
    {
        [Key]
        public int DriverKy { get; set; }

        [MaxLength(60)]
        public string DriverName { get; set; }

        [MaxLength(20)]
        public string? NIC { get; set; }

        [MaxLength(30)]
        public string? TP { get; set; }

        [MaxLength(30)]
        public string? LicenseNo { get; set; }

        public bool fInAct { get; set; }

        public int? EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }
    }
}
