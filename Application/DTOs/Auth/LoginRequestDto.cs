using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        public required string UserId { get; set; }
        public required string Password { get; set; }
        public required int CompanyKey { get; set; }
        public required int ProjectKey { get; set; } 
    }
}
