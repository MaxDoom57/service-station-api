using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Lookups
{
    /// <summary>
    /// vewItmCat2Cd class.
    /// </summary>
    public class vewItmCat2Cd
    {
        public short ItmCat2Ky { get; set; }
        public string ItmCat2Cd { get; set; } = null!;
        public string ItmCat2Nm { get; set; } = null!;
        public short CKy { get; set; }
        public bool fUsrAcs { get; set; }
    }
}
