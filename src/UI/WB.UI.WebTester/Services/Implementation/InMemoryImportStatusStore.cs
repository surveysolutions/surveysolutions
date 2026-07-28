using System;
using Microsoft.Extensions.Caching.Memory;

namespace WB.UI.WebTester.Services.Implementation
{
    /// <summary>
    /// Import-status store backed by <see cref="IMemoryCache"/> with an absolute TTL so that
    /// abandoned entries (e.g. the user closed the tab before <c>Loading()</c> cleared the
    /// status) are reclaimed automatically. The TTL matches the default delegated JWT lifetime
    /// (480 min) plus a 10-minute grace period; entries that are explicitly removed by
    /// <c>Loading()</c> are cleaned up immediately regardless of TTL.
    /// </summary>
    public class InMemoryImportStatusStore : IImportStatusStore
    {
        // 480 min (default JWT TTL) + 10 min grace so status is still readable at session end.
        private static readonly TimeSpan EntryTtl = TimeSpan.FromMinutes(490);

        private readonly IMemoryCache cache;

        public InMemoryImportStatusStore(IMemoryCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private static string Key(Guid interviewId) => "import:status:" + interviewId;

        public bool TryInitialize(Guid interviewId)
        {
            var key = Key(interviewId);
            // SetIfNotExist semantics: only add when not already present.
            if (cache.TryGetValue(key, out _))
                return false;
            cache.Set(key, CreationResult.Loading,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = EntryTtl });
            return true;
        }

        public void Set(Guid interviewId, CreationResult result)
            => cache.Set(Key(interviewId), result,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = EntryTtl });

        public CreationResult? Get(Guid interviewId)
            => cache.TryGetValue(Key(interviewId), out CreationResult result) ? result : null;

        public CreationResult? Remove(Guid interviewId)
        {
            var key = Key(interviewId);
            var existing = cache.TryGetValue(key, out CreationResult result) ? result : (CreationResult?)null;
            cache.Remove(key);
            return existing;
        }
    }
}
