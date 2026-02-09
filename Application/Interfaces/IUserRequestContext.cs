using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRequestContext
    {
        string UserId { get; set; }
        int CompanyKey { get; set; }
        int ProjectKey { get; set; }
    }
}
