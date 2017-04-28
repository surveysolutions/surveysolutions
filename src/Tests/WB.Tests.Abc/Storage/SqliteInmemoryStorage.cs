using NSubstitute;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Abc.Storage
{
    internal class SqliteInmemoryStorage<TEntity> : SqlitePlainStorage<TEntity> where TEntity : class, IPlainStorageEntity, new()
    {
        public SqliteInmemoryStorage() : base(
            new SQLiteConnectionWithLock(new SQLiteConnectionString(":memory:", true), openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex),
            Substitute.For<ILogger>())
        {
        }
    }
}