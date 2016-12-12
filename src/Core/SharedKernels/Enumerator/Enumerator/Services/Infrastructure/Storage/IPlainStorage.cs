using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage
{
    public interface IPlainStorage<TEntity> : IDisposable where TEntity : class, IPlainStorageEntity
    {
        TEntity GetById(string id);
        void Remove(IEnumerable<TEntity> entities);
        void Remove(string id);

        void Store(TEntity entity);
        void Store(IEnumerable<TEntity> entities);

        IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        IReadOnlyCollection<TEntity> FixedQuery(Expression<Func<TEntity, bool>> wherePredicate,
            Expression<Func<TEntity, int>> orderPredicate, int takeCount, int skip = 0);

        TEntity FirstOrDefault();
        IReadOnlyCollection<TEntity> LoadAll();
        int Count(Expression<Func<TEntity, bool>> predicate);
        void RemoveAll();
    }
}
