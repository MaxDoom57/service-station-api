using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Infrastructure.Services
{
    /// <summary>
    /// Implements sliding-window token activity tracking using IMemoryCache.
    /// Every time a token is touched the cache entry is renewed for another hour,
    /// so the session stays alive as long as the user keeps sending requests within 1 hour.
    /// </summary>
    public class TokenActivityService : ITokenActivityService
    {
        private readonly IMemoryCache _cache;

        // Prefix to avoid key collisions with the blacklist entries
        private const string Prefix = "tok_activity_";

        // Sliding window: token is considered "active" if a request arrived within this period
        private static readonly TimeSpan SlidingWindow = TimeSpan.FromHours(1);

        public TokenActivityService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <inheritdoc/>
        public void Touch(string token)
        {
            // Each call resets the sliding expiry to Now + 1 hour
            _cache.Set(Prefix + token, DateTime.UtcNow, new MemoryCacheEntryOptions
            {
                SlidingExpiration = SlidingWindow
            });
        }

        /// <inheritdoc/>
        public bool IsActiveWithinWindow(string token)
        {
            return _cache.TryGetValue(Prefix + token, out _);
        }

        /// <inheritdoc/>
        public void Remove(string token)
        {
            _cache.Remove(Prefix + token);
        }
    }
}
