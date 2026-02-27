using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        Task<UsrMas?> GetUserAsync(string userId, int cKy);
        Task<int?> GetUserKeyAsync(string userId, int cKy);
    }
}
