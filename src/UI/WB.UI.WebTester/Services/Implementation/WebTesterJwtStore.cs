using System;
using Microsoft.Extensions.Caching.Memory;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterJwtStore : IWebTesterJwtStore
    {
        private readonly IMemoryCache memoryCache;

        public WebTesterJwtStore(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        private static string CacheKey(Guid interviewId) => $"jwt-store:{interviewId}";

        public void StoreToken(Guid interviewId, string jwt, TimeSpan expiresIn)
        {
            // Subtract a small safety margin so the cached entry expires slightly
            // before the JWT itself — prevents sending a token that is valid now
            // but will be rejected by Designer within the same round-trip.
            var safeExpiry = expiresIn - TimeSpan.FromSeconds(30);
            if (safeExpiry <= TimeSpan.Zero)
                safeExpiry = expiresIn; // token is already very short-lived; use as-is

            memoryCache.Set(CacheKey(interviewId), jwt, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = safeExpiry
            });
        }

        public string? GetToken(Guid interviewId)
        {
            return memoryCache.Get<string>(CacheKey(interviewId));
        }

        public void Remove(Guid interviewId)
        {
            memoryCache.Remove(CacheKey(interviewId));
        }
    }
}
