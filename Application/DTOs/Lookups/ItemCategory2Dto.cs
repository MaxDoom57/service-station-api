using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Lookups
{
    public class ItemCategory2Dto
    {
        public short ItmCat2Ky { get; set; }
        public string ItmCat2Cd { get; set; } = null!;
        public string ItmCat2Nm { get; set; } = null!;
        public short CKy { get; set; }
        public bool fUsrAcs { get; set; }
    }
}
