using System;
using Microsoft.Extensions.Caching.Memory;

namespace WB.UI.WebTester.Services.Implementation
{
    /// <summary>
    /// Stores per-interview <see cref="RequestUserContext"/> in <see cref="IMemoryCache"/>
    /// with an absolute expiration equal to the delegated JWT lifetime.  Entries are
    /// therefore cleaned up automatically when the JWT expires, even if the session is
    /// abandoned and <see cref="Remove"/> is never called explicitly.
    /// </summary>
    public class InMemoryUserContextStore : IUserContextStore
    {
        private readonly IMemoryCache memoryCache;

        public InMemoryUserContextStore(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        private static string CacheKey(Guid interviewId) => $"user-ctx:{interviewId}";

        public void Store(Guid interviewId, RequestUserContext context, TimeSpan ttl)
            => memoryCache.Set(CacheKey(interviewId), context,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });

        public RequestUserContext? Get(Guid interviewId)
            => memoryCache.Get<RequestUserContext>(CacheKey(interviewId));

        public void Remove(Guid interviewId)
            => memoryCache.Remove(CacheKey(interviewId));
    }
}
