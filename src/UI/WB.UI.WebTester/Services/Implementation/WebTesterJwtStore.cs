using System;
using Microsoft.Extensions.Caching.Memory;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterJwtStore : IWebTesterJwtStore
    {
        private readonly IMemoryCache memoryCache;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(90);

        public WebTesterJwtStore(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        private static string CacheKey(Guid interviewId) => $"jwt-store:{interviewId}";

        public void StoreToken(Guid interviewId, string jwt)
        {
            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DefaultExpiration
            };
            memoryCache.Set(CacheKey(interviewId), jwt, entryOptions);
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
