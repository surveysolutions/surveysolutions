using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace WB.UI.Designer.Services
{
    /// <summary>
    /// In-memory implementation of <see cref="IOneTimeCodeStore"/> backed by <see cref="IMemoryCache"/>.
    /// Entries expire automatically via cache TTL — no background cleanup or lazy eviction required.
    /// <see cref="TryMarkAsUsedAsync"/> is atomic within a single process (lock on code string);
    /// for multi-replica deployments use a Redis/SQL-backed store.
    /// </summary>
    public class InMemoryOneTimeCodeStore : IOneTimeCodeStore
    {
        private static readonly TimeSpan UsedSentinelTtl = TimeSpan.FromMinutes(10);

        private readonly IMemoryCache cache;

        public InMemoryOneTimeCodeStore(IMemoryCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private static string DataKey(string code) => "otc:data:" + code;
        private static string UsedKey(string code) => "otc:used:" + code;

        public Task SaveAsync(OneTimeCodeEntity entity, CancellationToken ct = default)
        {
            var ttl = entity.ExpiresAt - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
                return Task.CompletedTask; // already expired — do not store

            cache.Set(DataKey(entity.Code), entity,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });

            return Task.CompletedTask;
        }

        public Task<OneTimeCodeEntity?> GetAsync(string code, CancellationToken ct = default)
        {
            cache.TryGetValue(DataKey(code), out OneTimeCodeEntity? entity);
            return Task.FromResult(entity);
        }

        public Task<bool> TryMarkAsUsedAsync(string code, DateTime usedAtUtc, CancellationToken ct = default)
        {
            // Lock on an interned string derived from the code to serialise concurrent callers
            // for the same code while not blocking callers for different codes.
            lock (string.Intern("otc:lock:" + code))
            {
                // Reject if already marked used.
                if (cache.TryGetValue(UsedKey(code), out _))
                    return Task.FromResult(false);

                if (!cache.TryGetValue(DataKey(code), out OneTimeCodeEntity? entity) || entity == null)
                    return Task.FromResult(false);

                if (entity.Used)
                    return Task.FromResult(false);

                entity.Used   = true;
                entity.UsedAt = usedAtUtc;

                // Remove the data entry immediately — it will never be needed again after a
                // successful exchange, and this reclaims memory without waiting for TTL expiry.
                cache.Remove(DataKey(code));

                // Keep a short-lived sentinel so a late duplicate call (within the grace window)
                // is still rejected rather than returning "not found → false" ambiguously.
                cache.Set(UsedKey(code), true,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = UsedSentinelTtl });

                return Task.FromResult(true);
            }
        }
    }
}
