using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Implementation
{
    public class InMemoryPlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity>
        where TEntity : class
    {
        private readonly Dictionary<object,TEntity> inMemroyStorage = new Dictionary<object, TEntity>(); 

        public TEntity GetById(object id)
        {
            if (this.inMemroyStorage.ContainsKey(id))
                return this.inMemroyStorage[id];
            return null;
        }

        public void Remove(object id)
        {
            if (this.inMemroyStorage.ContainsKey(id))
                this.inMemroyStorage.Remove(id);
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public void Store(TEntity entity, object id)
        {
            this.inMemroyStorage[id] = entity;
        }

        public void Store(IEnumerable<Tuple<TEntity, object>> entities)
        {
            foreach (var entity in entities)
            {
                this.Store(entity.Item1,entity.Item2);
            }
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return query.Invoke(this.inMemroyStorage.Values.AsQueryable());
        }
    }
}