using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class CreateUserDto
    {
        public string UsrNm { get; set; } = null!;
        public string UsrId { get; set; } = null!;
        public string NewPwd { get; set; } = null!;
        public string ConfirmPwd { get; set; } = null!;
        public string? PwdTip { get; set; }
    }
}
