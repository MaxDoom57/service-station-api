using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CompanyProject
    {
        [Key]
        public int CPKy { get; set; }

        public int CKy { get; set; }
        public int PrjKy { get; set; }
        public string DbServer { get; set; }
        public string DbName { get; set; }
        public string? DbUser { get; set; }
        public string? DbPassword { get; set; }
    }
}
