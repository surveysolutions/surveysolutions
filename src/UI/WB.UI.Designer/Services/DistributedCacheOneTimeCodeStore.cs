using System;
using System.Collections.Concurrent;
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
    // Atomicity guarantee (two layers):
    //
    //   Layer 1 — in-process ConcurrentDictionary.TryAdd (this class is registered as singleton).
    //             Lock-free and unconditionally atomic within a single replica.  All concurrent
    //             requests arriving at the same process instance are serialised here.
    //
    //   Layer 2 — distributed sentinel key (otc:used:{code}).
    //             Best-effort guard for requests that arrive at *different* replicas at the same
    //             moment.  IDistributedCache has no SET-NX primitive, so a very tight cross-replica
    //             race can still result in both callers proceeding past this check.
    //
    // For strict multi-replica atomicity (remove the residual cross-replica window) replace
    // IDistributedCache with IConnectionMultiplexer and execute a Lua script:
    //   local set = redis.call('SET', KEYS[1], '1', 'NX', 'PX', ARGV[1])
    //   return set and 1 or 0
    public class DistributedCacheOneTimeCodeStore : IOneTimeCodeStore
    {
        private readonly IDistributedCache cache;

        // In-process atomic gate.  Registered as singleton so this dictionary is shared across
        // all requests within the same process.  TryAdd is atomic and lock-free — exactly one
        // caller wins for each code value.
        private readonly ConcurrentDictionary<string, byte> localUsedCodes = new();

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
            // Layer 1: in-process atomic gate.
            // Only one caller per process can add a given code — all others return false immediately.
            if (!localUsedCodes.TryAdd(code, 1))
                return false;

            try
            {
                // Layer 2: distributed sentinel — reject if another replica already marked the code.
                // Not atomic with IDistributedCache (see class-level note), but reduces the cross-replica
                // window to the round-trip time of a single cache read.
                if (await cache.GetStringAsync(UsedKey(code), ct) != null)
                    return false;

                var json = await cache.GetStringAsync(DataKey(code), ct);
                if (json == null)
                    return false;

                var entity = JsonSerializer.Deserialize<OneTimeCodeEntity>(json);
                if (entity == null || entity.Used)
                    return false;

                // Grace period: keep entries visible after use so a late GetAsync
                // (e.g. for audit logging) still returns the full entity.
                var usedTtl = entity.ExpiresAt - DateTime.UtcNow + TimeSpan.FromMinutes(10);
                if (usedTtl <= TimeSpan.Zero) usedTtl = TimeSpan.FromMinutes(10);
                var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = usedTtl };

                // Write sentinel before updating entity so other replicas observe "used" as early
                // as possible, minimising the cross-replica window.
                await cache.SetStringAsync(UsedKey(code), "1", opts, ct);

                entity.Used = true;
                entity.UsedAt = usedAtUtc;
                await cache.SetStringAsync(DataKey(code), JsonSerializer.Serialize(entity), opts, ct);

                return true;
            }
            finally
            {
                localUsedCodes.TryRemove(code, out _);
            }
        }
    }
}
