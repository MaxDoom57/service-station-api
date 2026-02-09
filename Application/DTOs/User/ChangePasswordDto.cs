using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class ChangePasswordDto
    {
        public int UsrKy { get; set; }
        public string OldPwd { get; set; } = null!;
        public string NewPwd { get; set; } = null!;
        public string ConfirmPwd { get; set; } = null!;
        public string? PwdTip { get; set; }
    }
}
