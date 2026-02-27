using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Lookups
{
    public class ItemCategory1Dto
    {
        public short ItmCat1Ky { get; set; }
        public string ItmCat1Cd { get; set; } = string.Empty;
        public string ItmCat1Nm { get; set; } = string.Empty;
        public short CKy { get; set; }
    }
}
