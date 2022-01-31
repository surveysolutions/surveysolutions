using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;

namespace WB.Tests.Abc.Storage
{
    public class TestPlainStorage<T> : InMemoryPlainStorageAccessor<T> where T : class
    {
        public TestPlainStorage()
        {
        }

        public TestPlainStorage(Dictionary<object, T> entities) =>
            entities.ForEach(x => base.InMemoryStorage.TryAdd(x.Key, x.Value));
    }
}
