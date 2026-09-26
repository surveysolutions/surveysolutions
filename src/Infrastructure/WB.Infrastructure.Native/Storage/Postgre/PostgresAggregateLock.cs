#nullable enable
using System;
using System.Buffers.Binary;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
        private readonly AmbientUnitOfWorkAccessor ambientUnitOfWorkAccessor;
        private readonly Func<IDbConnection> connectionFactory;
        private readonly Action<IDbConnection, IDbTransaction, long> acquireTransactionScopedAdvisoryLock;
        private readonly NamedLocker localLocker = new NamedLocker();

        public PostgresAggregateLock(
            UnitOfWorkConnectionSettings connectionSettings,
            AmbientUnitOfWorkAccessor ambientUnitOfWorkAccessor)
            : this(
                ambientUnitOfWorkAccessor,
                () => new NpgsqlConnection(connectionSettings.ConnectionString),
                (connection, transaction, lockKey) =>
                    connection.Execute("SELECT pg_advisory_xact_lock(@key)", new { key = lockKey }, transaction))
        {
        }

        internal PostgresAggregateLock(
            AmbientUnitOfWorkAccessor ambientUnitOfWorkAccessor,
            Func<IDbConnection> connectionFactory,
            Action<IDbConnection, IDbTransaction, long> acquireTransactionScopedAdvisoryLock)
        {
            this.ambientUnitOfWorkAccessor = ambientUnitOfWorkAccessor;
            this.connectionFactory = connectionFactory;
            this.acquireTransactionScopedAdvisoryLock = acquireTransactionScopedAdvisoryLock;
        }

        public T RunWithLock<T>(string aggregateGuid, Func<T> run)
        {
            // Local lock: serializes concurrent requests on the same server instance,
            // preventing multiple threads from each opening a DB connection to wait for the advisory lock.
            return localLocker.RunWithLock(aggregateGuid, () =>
            {
                var lockKey = GetAdvisoryLockKey(aggregateGuid);

                var currentUnitOfWork = this.ambientUnitOfWorkAccessor.Current;
                if (currentUnitOfWork != null)
                {
                    currentUnitOfWork.Session
                        .CreateSQLQuery("SELECT pg_advisory_xact_lock(:key)")
                        .SetInt64("key", lockKey)
                        .UniqueResult();

                    return run();
                }

                var connection = this.connectionFactory();
                try
                {
                    connection.Open();

                    var transaction = connection.BeginTransaction();
                    Exception? executionException = null;

                    try
                    {
                        this.acquireTransactionScopedAdvisoryLock(connection, transaction, lockKey);
                        var result = run();
                        transaction.Commit();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        executionException = ex;
                        Rollback(connection, transaction, executionException);
                        throw;
                    }
                    finally
                    {
                        try
                        {
                            transaction.Dispose();
                        }
                        catch when (executionException != null)
                        {
                            ClearPool(connection);
                        }
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
                return ReadInt64LittleEndian(bytes, 0) ^ ReadInt64LittleEndian(bytes, 8);
            }

            // Fallback for non-GUID aggregate identifiers: use SHA-256 to get a full 64-bit hash
            // and avoid the narrow collision domain of a 32-bit GetHashCode().
            var stringBytes = Encoding.UTF8.GetBytes(aggregateGuid);
            var hash = SHA256.HashData(stringBytes);
            return ReadInt64LittleEndian(hash, 0);
        }

        private static long ReadInt64LittleEndian(byte[] bytes, int offset)
        {
            return BinaryPrimitives.ReadInt64LittleEndian(bytes.AsSpan(offset, sizeof(long)));
        }

        private static void ClearPool(IDbConnection connection)
        {
            if (connection is NpgsqlConnection npgsqlConnection)
                NpgsqlConnection.ClearPool(npgsqlConnection);
        }

        private static void Rollback(IDbConnection connection, IDbTransaction transaction, Exception executionException)
        {
            try
            {
                transaction.Rollback();
            }
            catch when (executionException != null)
            {
                ClearPool(connection);
            }
        }
    }

    public sealed class AmbientUnitOfWorkAccessor
    {
        private readonly AsyncLocal<IUnitOfWork?> current = new AsyncLocal<IUnitOfWork?>();

        public IUnitOfWork? Current => this.current.Value;

        public IDisposable Use(IUnitOfWork unitOfWork)
        {
            var previous = this.current.Value;
            this.current.Value = unitOfWork;
            return new RestoreCurrentUnitOfWork(this, previous);
        }

        private sealed class RestoreCurrentUnitOfWork : IDisposable
        {
            private readonly AmbientUnitOfWorkAccessor accessor;
            private readonly IUnitOfWork? previous;
            private bool disposed;

            public RestoreCurrentUnitOfWork(AmbientUnitOfWorkAccessor accessor, IUnitOfWork? previous)
            {
                this.accessor = accessor;
                this.previous = previous;
            }

            public void Dispose()
            {
                if (this.disposed)
                    return;

                this.accessor.current.Value = this.previous;
                this.disposed = true;
            }
        }
    }
}
