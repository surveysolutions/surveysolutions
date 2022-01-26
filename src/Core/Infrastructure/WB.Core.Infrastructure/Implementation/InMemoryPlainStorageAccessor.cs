using System;
using System.Collections.Concurrent;
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
        protected readonly ConcurrentDictionary<object, TEntity> InMemoryStorage = new ConcurrentDictionary<object, TEntity>(); 

        public TEntity GetById(object id)
        {
            return this.InMemoryStorage.ContainsKey(id) ? this.InMemoryStorage[id] : null;
        }

        public Task<TEntity> GetByIdAsync(object id)
        {
            return Task.FromResult(GetById(id));
        }

        public void Remove(object id)
        {
            this.InMemoryStorage.TryRemove(id, out _);
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities.ToList())
            {
                var itemToRemove = this.InMemoryStorage.SingleOrDefault(x => x.Value.Equals(entity));
                this.InMemoryStorage.TryRemove(itemToRemove.Key, out _);
            }
        }

        public void Remove(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
            => this.Remove(query.Invoke(this.InMemoryStorage.Values.AsQueryable()));

        public void Store(TEntity entity, object id)
        {
            if (id != null && this.InMemoryStorage.ContainsKey(id))
                this.InMemoryStorage[id] = entity;
            else
                this.InMemoryStorage.TryAdd(id ?? this.InMemoryStorage.Count + 1, entity);
        }

        public void Store(IEnumerable<Tuple<TEntity, object>> entities)
        {
            foreach (var entity in entities.ToList())
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
            return query.Invoke(this.InMemoryStorage.Values.AsQueryable());
        }

        public TEntity GetById(string id)
        {
            return GetById((object) id);
        }

        public bool HasNotEmptyValue(string id)
        {
            return this.InMemoryStorage.ContainsKey(id) && this.InMemoryStorage[id] != null;
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
