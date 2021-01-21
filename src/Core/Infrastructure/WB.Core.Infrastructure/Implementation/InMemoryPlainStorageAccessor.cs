using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Implementation
{
    public class InMemoryPlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity>, IPlainKeyValueStorage<TEntity>
        where TEntity : class
    {
        protected readonly Dictionary<object,TEntity> inMemoryStorage = new Dictionary<object, TEntity>(); 

        public TEntity GetById(object id)
        {
            if (this.inMemoryStorage.ContainsKey(id))
                return this.inMemoryStorage[id];
            return null;
        }

        public Task<TEntity> GetByIdAsync(object id)
        {
            return Task.FromResult(GetById(id));
        }

        public void Remove(object id)
        {
            if (this.inMemoryStorage.ContainsKey(id))
                this.inMemoryStorage.Remove(id);
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                var itemToRemove = this.inMemoryStorage.SingleOrDefault(x => x.Value.Equals(entity));
                this.inMemoryStorage.Remove(itemToRemove.Key);
            }
        }

        public void Remove(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
            => this.Remove(query.Invoke(this.inMemoryStorage.Values.AsQueryable()));

        public void Store(TEntity entity, object id)
        {
            if (id != null && this.inMemoryStorage.ContainsKey(id))
                this.inMemoryStorage[id] = entity;
            else
                this.inMemoryStorage.Add(id ?? this.inMemoryStorage.Count + 1, entity);
        }

        public void Store(IEnumerable<Tuple<TEntity, object>> entities)
        {
            foreach (var entity in entities)
            {
                this.Store(entity.Item1,entity.Item2);
            }
        }

        public void Store(IEnumerable<TEntity> entities) => entities.ForEach(x => this.Store(x, x));

        public void Flush()
        {
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return query.Invoke(this.inMemoryStorage.Values.AsQueryable());
        }

        public TEntity GetById(string id)
        {
            return GetById((object) id);
        }

        public bool HasNotEmptyValue(string id)
        {
            return this.inMemoryStorage.ContainsKey(id) && this.inMemoryStorage[id] != null;
        }

        public void Remove(string id)
        {
            Remove((object)id);
        }

        public void Store(TEntity view, string id)
        {
            Store(view, (object)id);
        }
    }
}
