using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage
{
    public interface IAsyncPlainStorage<TEntity> : IDisposable where TEntity : class, IPlainStorageEntity
    {
        TEntity GetById(string id);
        Task<TEntity> GetByIdAsync(string id);
        Task RemoveAsync(string id);
        Task RemoveAsync(IEnumerable<TEntity> entities);
        void Remove(IEnumerable<TEntity> entities);

        Task StoreAsync(TEntity entity);
        Task StoreAsync(IEnumerable<TEntity> entities);

        IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
        Task<IReadOnlyCollection<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);

        IReadOnlyCollection<TEntity> FixedQuery(Expression<Func<TEntity, bool>> wherePredicate,
            Expression<Func<TEntity, int>> orderPredicate, int takeCount);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity FirstOrDefault();
        IReadOnlyCollection<TEntity> LoadAll();
        Task<IReadOnlyCollection<TEntity>> LoadAllAsync();
        int Count(Expression<Func<TEntity, bool>> predicate);
        Task RemoveAllAsync();
    }
}
