using System;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgresPlainKeyValueStorage<TEntity> : PostgresKeyValueStorage<TEntity>,
        IPlainKeyValueStorage<TEntity>, IDisposable
        where TEntity : class
    {
        public PostgresPlainKeyValueStorage(IPlainSessionProvider plainSessionProvider, PostgresPlainStorageSettings connectionSettings)
            : base(plainSessionProvider, connectionSettings.ConnectionString)
        {
        }
    }
}