using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public class InMemoryPlainStorage<TEntity> : IPlainStorage<TEntity>
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

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return this.inMemroyStorage.Values.AsQueryable().FirstOrDefault(predicate);
        }

        public IReadOnlyCollection<TEntity> LoadAll()
        {
            return this.inMemroyStorage.Values.ToReadOnlyCollection();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return this.inMemroyStorage.Values.AsQueryable().Count(predicate);
        }

        public int Count() => this.inMemroyStorage.Values.Count;

        public void RemoveAll()
        {
            this.inMemroyStorage.Clear();
        }

        public IReadOnlyCollection<TEntity> FixedQuery(Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, int>> orderPredicate, int takeCount, int skip = 0)
        {
            return this.inMemroyStorage.Values.AsQueryable().Where(wherePredicate).OrderBy(orderPredicate).Skip(skip).Take(takeCount).ToReadOnlyCollection();
        }

        public IReadOnlyCollection<TResult> FixedQueryWithSelection<TResult>(
            Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, int>> orderPredicate,
            Expression<Func<TEntity, TResult>> selectPredicate, int takeCount, int skip = 0)
            where TResult : class 
            => this.inMemroyStorage.Values.AsQueryable().Where(wherePredicate).OrderBy(orderPredicate).Select(selectPredicate).Skip(skip).Take(takeCount).ToReadOnlyCollection();

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
