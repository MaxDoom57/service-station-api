using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;


namespace Infrastructure.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;

        public TokenBlacklistService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void BlacklistToken(string token, DateTime expiration)
        {
            var duration = expiration - DateTime.UtcNow;
            if (duration.TotalSeconds <= 0)
                duration = TimeSpan.FromMinutes(1);

            _cache.Set(token, true, duration);
        }

        public bool IsTokenBlacklisted(string token)
        {
            return _cache.TryGetValue(token, out _);
        }
    }
}
