#nullable enable
using System;
using Dapper;
using Npgsql;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    /// <summary>
    /// PostgreSQL advisory lock-based implementation of <see cref="IAggregateLock"/>.
    /// Supports farm mode (multi-server deployments) by using database-level advisory locks
    /// that are visible across all server instances sharing the same PostgreSQL instance.
    /// Uses a local in-process lock as a first layer to avoid unnecessary DB round-trips
    /// when multiple threads on the same server compete for the same aggregate.
    /// </summary>
    public class PostgresAggregateLock : IAggregateLock
    {
        private readonly string connectionString;
        private readonly NamedLocker localLocker = new NamedLocker();

        public PostgresAggregateLock(UnitOfWorkConnectionSettings connectionSettings)
        {
            this.connectionString = connectionSettings.ConnectionString;
        }

        public T RunWithLock<T>(string aggregateGuid, Func<T> run)
        {
            // Local lock: serializes concurrent requests on the same server instance,
            // preventing multiple threads from each opening a DB connection to wait for the advisory lock.
            return localLocker.RunWithLock(aggregateGuid, () =>
            {
                var lockKey = GetAdvisoryLockKey(aggregateGuid);

                var connection = new NpgsqlConnection(connectionString);
                try
                {
                    connection.Open();

                    // Acquire PostgreSQL session-level advisory lock.
                    // Blocks until the lock is available (cross-server synchronization).
                    connection.Execute("SELECT pg_advisory_lock(@key)", new { key = lockKey });
                    try
                    {
                        return run();
                    }
                    finally
                    {
                        // Release the advisory lock explicitly before closing the connection
                        // to ensure deterministic unlock and avoid spurious errors on connection close.
                        connection.Execute("SELECT pg_advisory_unlock(@key)", new { key = lockKey });
                    }
                }
                finally
                {
                    connection.Dispose();
                }
            });
        }

        public void RunWithLock(string aggregateGuid, Action run)
        {
            RunWithLock<object?>(aggregateGuid, () => { run(); return null; });
        }

        /// <summary>
        /// Converts an aggregate GUID string to a stable 64-bit integer key for use with
        /// PostgreSQL advisory locks. XORs the two 8-byte halves of the GUID bytes to
        /// produce a well-distributed value. For non-GUID strings, uses SHA-256 to derive
        /// a full 64-bit value, avoiding the narrow collision domain of a 32-bit hash.
        /// </summary>
        internal static long GetAdvisoryLockKey(string aggregateGuid)
        {
            if (Guid.TryParse(aggregateGuid, out var guid))
            {
                var bytes = guid.ToByteArray();
                return BitConverter.ToInt64(bytes, 0) ^ BitConverter.ToInt64(bytes, 8);
            }

            // Fallback for non-GUID aggregate identifiers: use SHA-256 to get a full 64-bit hash
            // and avoid the narrow collision domain of a 32-bit GetHashCode().
            var stringBytes = System.Text.Encoding.UTF8.GetBytes(aggregateGuid);
            var hash = System.Security.Cryptography.SHA256.HashData(stringBytes);
            return BitConverter.ToInt64(hash, 0);
        }
    }
}
