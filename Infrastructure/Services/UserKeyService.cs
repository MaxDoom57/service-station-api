using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UserKeyService : IUserKeyService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;

        public UserKeyService(IDynamicDbContextFactory factory, IUserRequestContext userContext)
        {
            _factory = factory;
            _userContext = userContext;
        }

        public async Task<int?> GetUserKeyAsync(string userId, int cKy)
        {
            using var db = await _factory.CreateDbContextAsync();

            var userKey = await db.UsrMas
                .Where(x => x.UsrId == userId && x.CKy == cKy)
                .Select(x => (int?)x.UsrKy)
                .FirstOrDefaultAsync();

            return userKey;
        }
    }
}
