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

        Task RemoveAsync(string id);
        Task RemoveAsync(IEnumerable<TEntity> entities);

        Task StoreAsync(TEntity entity);
        Task StoreAsync(IEnumerable<TEntity> entities);

        IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
        int Count(Expression<Func<TEntity, bool>> predicate);
        TEntity FirstOrDefault();
        Task<TEntity> FirstOrDefaultAsync();
        IList<TEntity> LoadAll();
        Task<IList<TEntity>> LoadAllAsync();
    }
}
