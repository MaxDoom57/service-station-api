using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// LoginResponseDto class.
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
