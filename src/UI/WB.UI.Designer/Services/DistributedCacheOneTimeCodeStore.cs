using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace WB.UI.Designer.Services
{
    // IOneTimeCodeStore backed by IDistributedCache (data) + PostgreSQL (atomic mark-as-used).
    //
    // Atomicity guarantee (two layers):
    //
    //   Layer 1 — in-process ConcurrentDictionary.TryAdd (this class is registered as singleton).
    //             Lock-free and unconditionally atomic within a single replica.
    //
    //   Layer 2 — PostgreSQL INSERT … ON CONFLICT DO NOTHING on the used_one_time_codes table.
    //             The PRIMARY KEY constraint on `code` guarantees exactly-once semantics across
    //             all replicas sharing the same database.  Only the first INSERT succeeds;
    //             concurrent inserts on other replicas return 0 affected rows.
    public class DistributedCacheOneTimeCodeStore : IOneTimeCodeStore
    {
        private readonly IDistributedCache cache;
        private readonly string connectionString;
        private readonly ILogger<DistributedCacheOneTimeCodeStore> logger;

        private readonly ConcurrentDictionary<string, byte> localUsedCodes = new();

        private static long lastCleanupTicks;

        public DistributedCacheOneTimeCodeStore(
            IDistributedCache cache,
            IConfiguration configuration,
            ILogger<DistributedCacheOneTimeCodeStore> logger)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is required for one-time code store.");
        }

        private static string DataKey(string code) => "otc:data:" + code;

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
            if (!localUsedCodes.TryAdd(code, 1))
                return false;

            try
            {
                var json = await cache.GetStringAsync(DataKey(code), ct);
                if (json == null)
                    return false;

                var entity = JsonSerializer.Deserialize<OneTimeCodeEntity>(json);
                if (entity == null || entity.Used)
                    return false;

                // Layer 2: PostgreSQL atomic guard.
                // INSERT … ON CONFLICT DO NOTHING returns 1 affected row only for the first caller.
                bool inserted = await TryInsertUsedCodeAsync(code, usedAtUtc, ct);
                if (!inserted)
                    return false;

                // Update the cached entity to reflect the used state.
                var usedTtl = entity.ExpiresAt - DateTime.UtcNow + TimeSpan.FromMinutes(10);
                if (usedTtl <= TimeSpan.Zero) usedTtl = TimeSpan.FromMinutes(10);
                var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = usedTtl };

                entity.Used = true;
                entity.UsedAt = usedAtUtc;
                await cache.SetStringAsync(DataKey(code), JsonSerializer.Serialize(entity), opts, ct);

                // Lazy cleanup of expired rows (at most once per hour).
                _ = TryCleanupExpiredRowsAsync();

                return true;
            }
            finally
            {
                localUsedCodes.TryRemove(code, out _);
            }
        }

        private async Task<bool> TryInsertUsedCodeAsync(string code, DateTime usedAtUtc, CancellationToken ct)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "INSERT INTO used_one_time_codes (code, used_at) VALUES ($1, $2) ON CONFLICT DO NOTHING";
            cmd.Parameters.Add(new NpgsqlParameter { Value = code });
            cmd.Parameters.Add(new NpgsqlParameter { Value = usedAtUtc });

            var affected = await cmd.ExecuteNonQueryAsync(ct);
            return affected == 1;
        }

        private async Task TryCleanupExpiredRowsAsync()
        {
            var now = DateTime.UtcNow.Ticks;
            var last = Interlocked.Read(ref lastCleanupTicks);
            if (now - last < TimeSpan.TicksPerHour)
                return;

            if (Interlocked.CompareExchange(ref lastCleanupTicks, now, last) != last)
                return;

            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM used_one_time_codes WHERE used_at < $1";
                cmd.Parameters.Add(new NpgsqlParameter { Value = DateTime.UtcNow.AddHours(-1) });
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to clean up expired used_one_time_codes rows");
            }
        }
    }
}
