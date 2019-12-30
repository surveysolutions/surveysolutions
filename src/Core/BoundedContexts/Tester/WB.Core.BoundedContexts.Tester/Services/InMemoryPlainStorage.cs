using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public class InMemoryPlainStorage<TEntity> : InMemoryPlainStorage<TEntity, string>, IPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity
    {

    }


    public class InMemoryPlainStorage<TEntity, TKey> : IPlainStorage<TEntity, TKey>
        where TEntity : class, IPlainStorageEntity<TKey>
    {
        public readonly List<TEntity> inMemroyStorage = new List<TEntity>();

        public TEntity GetById(TKey id)
        {
            return this.inMemroyStorage.FirstOrDefault(entity => EqualityComparer<TKey>.Default.Equals(entity.Id, id));
        }

        public IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return this.inMemroyStorage.AsQueryable().Where(predicate).ToReadOnlyCollection();
        }

        public IReadOnlyCollection<TResult> WhereSelect<TResult>(Expression<Func<TEntity, bool>> wherePredicate,
            Expression<Func<TEntity, TResult>> selectPredicate) where TResult : class
            => this.inMemroyStorage.AsQueryable().Where(wherePredicate).Select(selectPredicate).ToReadOnlyCollection();

        public TEntity FirstOrDefault()
        {
            return this.inMemroyStorage.AsQueryable().FirstOrDefault();
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return this.inMemroyStorage.AsQueryable().FirstOrDefault(predicate);
        }

        public IReadOnlyCollection<TEntity> LoadAll()
        {
            return this.inMemroyStorage.ToReadOnlyCollection();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return this.inMemroyStorage.AsQueryable().Count(predicate);
        }

        public int Count() => this.inMemroyStorage.Count;

        public void RemoveAll()
        {
            this.inMemroyStorage.Clear();
        }

        public IReadOnlyCollection<TEntity> FixedQuery(Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, int>> orderPredicate, int takeCount, int skip = 0)
        {
            return this.inMemroyStorage.AsQueryable().Where(wherePredicate).OrderBy(orderPredicate).Skip(skip).Take(takeCount).ToReadOnlyCollection();
        }

        public IReadOnlyCollection<TResult> FixedQueryWithSelection<TResult>(
            Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, int>> orderPredicate,
            Expression<Func<TEntity, TResult>> selectPredicate, int takeCount, int skip = 0)
            where TResult : class 
            => this.inMemroyStorage.AsQueryable().Where(wherePredicate).OrderBy(orderPredicate).Select(selectPredicate).Skip(skip).Take(takeCount).ToReadOnlyCollection();

        public void Remove(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities.Where(entity => entity != null))
            {
                this.inMemroyStorage.Remove(entity);
            }
        }

        public void Remove(TKey id)
        {
            this.inMemroyStorage.RemoveAll(entity => EqualityComparer<TKey>.Default.Equals(entity.Id, id));
        }

        public void Store(TEntity entity)
        {
            this.inMemroyStorage.Add(entity);
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
