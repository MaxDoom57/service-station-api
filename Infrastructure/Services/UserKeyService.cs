using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UserKeyService : IUserKeyService
    {
        private readonly IUserRepository _repo;

        public UserKeyService(IUserRepository repo)
        {
            _repo = repo;
        }

        public Task<int?> GetUserKeyAsync(string userId, int cKy)
        {
            return _repo.GetUserKeyAsync(userId, cKy);
        }
    }
}
