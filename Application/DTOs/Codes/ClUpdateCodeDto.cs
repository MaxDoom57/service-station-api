using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Codes
{
    public class UpdateCodeDto
    {
        public string Code { get; set; } = string.Empty;
        public string? CdNm { get; set; }
        public bool fInAct { get; set; }
    }
}
