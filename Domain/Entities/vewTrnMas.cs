using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class vewTrnMas
    {
        public short CKy { get; set; }
        public int TrnKy { get; set; }
        public int TrnNo { get; set; }

        [MaxLength(10)]
        public string? OurCd { get; set; }

        [MaxLength(15)]
        public string? DocNo { get; set; }

        [MaxLength(15)]
        public string? YurRef { get; set; }

        public short TrnTypKy { get; set; }

        public bool? fPrint { get; set; }
    }
}
