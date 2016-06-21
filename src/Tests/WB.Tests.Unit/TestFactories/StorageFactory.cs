using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.TestFactories
{
    internal class StorageFactory
    {
        public IPlainStorageAccessor<TEntity> InMemoryPlainStorage<TEntity>() where TEntity : class => new InMemoryPlainStorageAccessor<TEntity>();
    }
}