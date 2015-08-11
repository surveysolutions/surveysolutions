using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Tester.Services.Infrastructure
{
    public interface IAsyncPlainStorage<TEntity> where TEntity : class, IPlainStorageEntity
    {
        TEntity GetById(string id);

        Task RemoveAsync(string id);
        Task RemoveAsync(IEnumerable<TEntity> entities);

        Task StoreAsync(TEntity entity);
        Task StoreAsync(IEnumerable<TEntity> entities);

        TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query);
    }
}
