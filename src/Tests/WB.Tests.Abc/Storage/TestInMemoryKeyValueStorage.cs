using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Abc.Storage
{
    public class TestInMemoryKeyValueStorage<TEntity> : IPlainKeyValueStorage<TEntity> where TEntity : class
    {
        public readonly Dictionary<string, TEntity> Storage = new Dictionary<string, TEntity>();

        public TestInMemoryKeyValueStorage()
        {
        }

        public TestInMemoryKeyValueStorage(Dictionary<string, TEntity> initialState)
        {
            this.Storage = initialState;
        }

        public TEntity GetById(string id)
        {
            if (this.Storage.ContainsKey(id))
                return this.Storage[id];
            return null;
        }

        public bool HasNotEmptyValue(string id)
        {
            return this.Storage.ContainsKey(id);
        }

        public void Remove(string id)
        {
            if (this.Storage.ContainsKey(id))
                this.Storage.Remove(id);
        }

        public void Store(TEntity view, string id)
        {
            this.Storage[id] = view;
        }

        public TEntity ValueAt(int index)
        {
            return Storage.Values.ElementAt(index);
        }
    }
}
