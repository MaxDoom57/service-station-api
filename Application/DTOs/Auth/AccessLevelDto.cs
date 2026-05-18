using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// AccessLevelDto class.
    /// </summary>
    public class AccessLevelDto
    {
        public bool fAcs { get; set; }
        public bool fNew { get; set; }
        public bool fUpdt { get; set; }
        public bool fDel { get; set; }
        public bool fSp { get; set; }
    }

}
