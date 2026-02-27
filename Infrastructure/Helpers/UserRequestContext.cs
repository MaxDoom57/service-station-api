using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Infrastructure.Helpers
{
    public class UserRequestContext : IUserRequestContext
    {
        public string UserId { get; set; }
        public int CompanyKey { get; set; }
        public int ProjectKey { get; set; }
    }
}
