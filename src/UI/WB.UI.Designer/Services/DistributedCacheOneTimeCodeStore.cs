using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace WB.UI.Designer.Services
{
    // IOneTimeCodeStore backed by IDistributedCache.
    // Works across multiple replicas when a shared provider (Redis, SQL) is configured.
    // Falls back to in-process memory when only AddDistributedMemoryCache is registered.
    //
    // Atomicity note: IDistributedCache has no built-in CAS primitive.
    // TryMarkAsUsedAsync writes a sentinel key before updating the entity, which minimises
    // the race window to the RTT of one cache write. For strict guarantees in
    // high-throughput scenarios replace with a Redis Lua script (SET NX) or a
    // SQL UPDATE ... WHERE Used = 0 returning the number of affected rows.
    public class DistributedCacheOneTimeCodeStore : IOneTimeCodeStore
    {
        private readonly IDistributedCache cache;

        public DistributedCacheOneTimeCodeStore(IDistributedCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private static string DataKey(string code) => "otc:data:" + code;
        private static string UsedKey(string code) => "otc:used:" + code;

        public async Task SaveAsync(OneTimeCodeEntity entity, CancellationToken ct = default)
        {
            var ttl = entity.ExpiresAt - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromSeconds(5);

            await cache.SetStringAsync(
                DataKey(entity.Code),
                JsonSerializer.Serialize(entity),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                ct);
        }

        public async Task<OneTimeCodeEntity?> GetAsync(string code, CancellationToken ct = default)
        {
            var json = await cache.GetStringAsync(DataKey(code), ct);
            return json == null ? null : JsonSerializer.Deserialize<OneTimeCodeEntity>(json);
        }

        public async Task<bool> TryMarkAsUsedAsync(string code, DateTime usedAtUtc, CancellationToken ct = default)
        {
            // Fast-path: sentinel present -> code was already used
            if (await cache.GetStringAsync(UsedKey(code), ct) != null)
                return false;

            var json = await cache.GetStringAsync(DataKey(code), ct);
            if (json == null) return false;

            var entity = JsonSerializer.Deserialize<OneTimeCodeEntity>(json);
            if (entity == null || entity.Used) return false;

            // Grace period: keep entries visible after use so a late GetAsync
            // (e.g. for audit logging) still returns the full entity.
            var usedTtl = entity.ExpiresAt - DateTime.UtcNow + TimeSpan.FromMinutes(10);
            if (usedTtl <= TimeSpan.Zero) usedTtl = TimeSpan.FromMinutes(10);
            var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = usedTtl };

            // Write sentinel first — concurrent readers see it before the entity is updated.
            await cache.SetStringAsync(UsedKey(code), "1", opts, ct);

            entity.Used = true;
            entity.UsedAt = usedAtUtc;
            await cache.SetStringAsync(DataKey(code), JsonSerializer.Serialize(entity), opts, ct);

            return true;
        }
    }
}

