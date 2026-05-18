using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Codes
{
    /// <summary>
    /// CreateCodeDto class.
    /// </summary>
    public class CreateCodeDto
    {
        public string Code { get; set; } = string.Empty;
        public string? CdNm { get; set; }
        public string ConCd { get; set; } = string.Empty;
    }
}
