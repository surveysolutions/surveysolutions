using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
namespace WB.UI.Designer.Services
{
    /// <summary>
    /// In-memory implementation of <see cref="IOneTimeCodeStore"/>.
    /// TryMarkAsUsedAsync uses ConcurrentDictionary.TryAdd which is atomic,
    /// guaranteeing exactly-once semantics under concurrent load.
    /// </summary>
    /// <remarks>
    /// WARNING: suitable only for a single-process deployment.
    /// For multi-replica production deployments use a Redis or SQL-backed implementation.
    /// Expired entries are lazily evicted on read; for high-throughput scenarios
    /// consider adding a periodic background cleanup via IHostedService.
    /// </remarks>
    public class InMemoryOneTimeCodeStore : IOneTimeCodeStore
    {
        private readonly ConcurrentDictionary<string, OneTimeCodeEntity> store = new();
        // Separate "used" set — TryAdd gives atomic CAS semantics.
        private readonly ConcurrentDictionary<string, byte> usedSet = new();

        public Task SaveAsync(OneTimeCodeEntity entity, CancellationToken ct = default)
        {
            store[entity.Code] = entity;
            return Task.CompletedTask;
        }

        public Task<OneTimeCodeEntity?> GetAsync(string code, CancellationToken ct = default)
        {
            store.TryGetValue(code, out var entity);

            // Lazy eviction: remove entries that are well past their expiry.
            if (entity != null && entity.UsedAt.HasValue
                && entity.UsedAt.Value < DateTime.UtcNow.AddMinutes(-10))
            {
                store.TryRemove(code, out _);
                usedSet.TryRemove(code, out _);
            }

            return Task.FromResult(entity);
        }
        public Task<bool> TryMarkAsUsedAsync(string code, DateTime usedAtUtc, CancellationToken ct = default)
        {
            if (!store.TryGetValue(code, out var entity))
                return Task.FromResult(false);
            // TryAdd is atomic: only the FIRST caller succeeds.
            if (!usedSet.TryAdd(code, 0))
                return Task.FromResult(false);
            entity.Used = true;
            entity.UsedAt = usedAtUtc;
            return Task.FromResult(true);
        }
    }
}
