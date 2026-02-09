using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ServiceStatus")]
    public class ServiceStatus
    {
        [Key]
        public int TempServiceKy { get; set; }
        public int CKy { get; set; }
        public string VehicleId { get; set; }
        public string ServiceNm { get; set; } // Maps to VehicleId concept based on user requirement
        public bool fFinish { get; set; } // bit maps to bool
        public bool fTerminate { get; set; } // bit maps to bool
    }
}
