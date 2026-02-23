using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserKeyService
    {
        Task<int?> GetUserKeyAsync(string userId, int cKy);
    }
}
