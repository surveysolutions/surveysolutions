using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Implementation
{
    public class InMemoryPlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity>
        where TEntity : class
    {
        private readonly Dictionary<string,TEntity> inMemroyStorage=new Dictionary<string, TEntity>(); 
        public TEntity GetById(string id)
        {
            if (this.inMemroyStorage.ContainsKey(id))
                return this.inMemroyStorage[id];
            return null;
        }

        public void Remove(string id)
        {
            if (this.inMemroyStorage.ContainsKey(id))
                this.inMemroyStorage.Remove(id);
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public void Store(TEntity entity, string id)
        {
            this.inMemroyStorage[id] = entity;
        }

        public void Store(IEnumerable<Tuple<TEntity, string>> entities)
        {
            foreach (var entity in entities)
            {
                this.Store(entity.Item1,entity.Item2);
            }
        }
    }
}