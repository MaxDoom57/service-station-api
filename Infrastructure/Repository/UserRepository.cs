using Application.Interfaces;
using Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly MainDbContext _db;

        public UserRepository(MainDbContext db)
        {
            _db = db;
        }

        public async Task<UsrMas?> GetUserAsync(string userId, int cKy)
        {
            return await _db.UserMas
                .FirstOrDefaultAsync(x => x.UsrId == userId && x.CKy == cKy);
        }

        public async Task<int?> GetUserKeyAsync(string userId, int cKy)
        {
            var user = await _db.UserMas
                .Where(x => x.UsrId == userId && x.CKy == cKy)
                .Select(x => (int?)x.UsrKy)
                .FirstOrDefaultAsync();

            return user;
        }
    }
}
