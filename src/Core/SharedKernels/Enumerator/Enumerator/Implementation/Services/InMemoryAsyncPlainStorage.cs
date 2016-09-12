using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class InMemoryAsyncPlainStorage<TEntity> : IAsyncPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity
    {
        public readonly Dictionary<string, TEntity> inMemroyStorage = new Dictionary<string, TEntity>();

        public TEntity GetById(string id)
        {
            return this.inMemroyStorage.ContainsKey(id) ? this.inMemroyStorage[id] : null;
        }

        public IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return this.inMemroyStorage.Values.AsQueryable().Where(predicate).ToReadOnlyCollection();
        }

        public TEntity FirstOrDefault()
        {
            return this.inMemroyStorage.Values.AsQueryable().FirstOrDefault();
        }

        public IReadOnlyCollection<TEntity> LoadAll()
        {
            return this.inMemroyStorage.Values.ToReadOnlyCollection();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return this.inMemroyStorage.Values.AsQueryable().Count(predicate);
        }

        public void RemoveAll()
        {
            this.inMemroyStorage.Clear();
        }

        public IReadOnlyCollection<TEntity> FixedQuery(Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, int>> orderPredicate, int takeCount)
        {
            return this.inMemroyStorage.Values.AsQueryable().Where(wherePredicate).OrderBy(orderPredicate).Take(takeCount).ToReadOnlyCollection();
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities.Where(entity => entity != null))
            {
                if (this.inMemroyStorage.ContainsKey(entity.Id))
                    this.inMemroyStorage.Remove(entity.Id);
            }
        }

        public void Remove(string id)
        {
            if (this.inMemroyStorage.ContainsKey(id)) this.inMemroyStorage.Remove(id);
        }

        public void Store(TEntity entity)
        {
            this.inMemroyStorage[entity.Id] = entity;
        }

        public void Store(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities.Where(entity => entity != null))
            {
                this.Store(entity);
            }
        }

        public void Dispose()
        {
            this.inMemroyStorage.Clear();
        }
    }
}