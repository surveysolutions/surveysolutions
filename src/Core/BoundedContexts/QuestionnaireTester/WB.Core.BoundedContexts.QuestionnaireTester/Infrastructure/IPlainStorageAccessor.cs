using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface IPlainStorageAccessor<TEntity> where TEntity : class, IPlainStorageEntity
    {
        Task<TEntity> GetByIdAsync(string id);

        Task RemoveAsync(string id);
        Task RemoveAsync(IEnumerable<TEntity> entities);

        Task StoreAsync(TEntity entity);
        Task StoreAsync(IEnumerable<TEntity> entities);

        Task<TResult> QueryAsync<TResult>(Func<IQueryable<TEntity>, TResult> query);
    }
}
