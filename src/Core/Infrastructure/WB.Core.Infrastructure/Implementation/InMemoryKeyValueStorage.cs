using System.Collections.Generic;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Implementation
{
    public class InMemoryKeyValueStorage<TEntity> : IPlainKeyValueStorage<TEntity> where TEntity : class
    {
        private readonly Dictionary<string,TEntity> inMemoryStorage = new Dictionary<string, TEntity>(); 
        
        public TEntity GetById(string id)
        {
            if (this.inMemoryStorage.ContainsKey(id))
                return this.inMemoryStorage[id];
            return null;
        }

        public void Remove(string id)
        {
            if (this.inMemoryStorage.ContainsKey(id))
                this.inMemoryStorage.Remove(id);
        }

        public void Store(TEntity view, string id)
        {
            this.inMemoryStorage[id] = view;
        }
    }
}