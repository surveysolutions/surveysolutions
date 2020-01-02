using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SQLite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public class InMemoryPlainStorage<TEntity> : InMemoryPlainStorage<TEntity, string>, IPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity, new()
    {
        public InMemoryPlainStorage(ILogger logger) : base(logger)
        {
        }
    }

    public class InMemoryPlainStorage<TEntity, TKey> : SqlitePlainStorage<TEntity, TKey> where TEntity : class, IPlainStorageEntity<TKey>, new()
    {
        public InMemoryPlainStorage(ILogger logger) : base(
            new SQLiteConnectionWithLock(new SQLiteConnectionString(":memory:")),
            logger)
        {
        }
    }
}
