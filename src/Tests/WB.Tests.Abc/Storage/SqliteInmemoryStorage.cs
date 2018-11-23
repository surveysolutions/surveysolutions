using NSubstitute;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Abc.Storage
{
    internal class SqliteInmemoryStorage<TEntity, TKey> : SqlitePlainStorage<TEntity, TKey> where TEntity : class, IPlainStorageEntity<TKey>, new()
    {
        public SqliteInmemoryStorage() : base(
            new SQLiteConnectionWithLock(new SQLiteConnectionString(":memory:", true, null), openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex),
            Substitute.For<ILogger>())
        {
        }
    }

    internal class SqliteInmemoryStorage<TEntity> : SqliteInmemoryStorage<TEntity, string>, IPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity, IPlainStorageEntity<string>, new()
    {
    }
}
