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
    public class InMemoryOneTimeCodeStore : IOneTimeCodeStore
    {
        private readonly ConcurrentDictionary<string, OneTimeCodeEntity> store = new();
        // Separate "used" set so TryAdd gives us atomic CAS semantics.
        private readonly ConcurrentDictionary<string, byte> usedSet = new();
        public Task SaveAsync(OneTimeCodeEntity entity, CancellationToken ct = default)
        {
            store[entity.Code] = entity;
            return Task.CompletedTask;
        }
        public Task<OneTimeCodeEntity?> GetAsync(string code, CancellationToken ct = default)
        {
            store.TryGetValue(code, out var entity);
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
