using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage
{
    public interface IPlainStorage<TEntity> : IPlainStorage<TEntity, string>
        where TEntity : class, IPlainStorageEntity, IPlainStorageEntity<string>
    {
    }

    public interface IPlainStorage<TEntity, in TKey> : IDisposable where TEntity : class, IPlainStorageEntity<TKey>
    {
        TEntity GetById(TKey id);
        void Remove(IEnumerable<TEntity> entities);
        void Remove(TKey id);

        void Store(TEntity entity);
        void Store(IEnumerable<TEntity> entities);

        IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        IReadOnlyCollection<TResult> WhereSelect<TResult>(Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> selectPredicate) where TResult : class;

        IReadOnlyCollection<TEntity> FixedQuery(Expression<Func<TEntity, bool>> wherePredicate,
            Expression<Func<TEntity, int>> orderPredicate, int takeCount, int skip = 0);

        IReadOnlyCollection<TResult> FixedQueryWithSelection<TResult>(Expression<Func<TEntity, bool>> wherePredicate,
            Expression<Func<TEntity, int>> orderPredicate, Expression<Func<TEntity, TResult>> selectPredicate,
            int takeCount, int skip = 0) where TResult : class;

        TEntity FirstOrDefault();
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);
        IReadOnlyCollection<TEntity> LoadAll();
        int Count(Expression<Func<TEntity, bool>> predicate);
        int Count();
        void RemoveAll();
    }
}
