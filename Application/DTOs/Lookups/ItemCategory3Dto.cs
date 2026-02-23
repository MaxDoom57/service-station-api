using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Lookups
{
    public class ItemCategory3Dto
    {
        public short ItmCat3Ky { get; set; }
        public string ItmCat3Cd { get; set; } = null!;
        public string ItmCat3Nm { get; set; } = null!;
        public short CKy { get; set; }
    }
}
