using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IPlainStorageAccessor<TEntity> where TEntity : class
    {
        TEntity GetById(object id);
        Task<TEntity> GetByIdAsync(object id);

        void Remove(object id);
        void Remove(IEnumerable<TEntity> entities);
        void Remove(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);

        void Store(TEntity entity, object id);

        void Store(IEnumerable<Tuple<TEntity, object>> entities);
        void Store(IEnumerable<TEntity> entities);

        void Flush();

        TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query);
    }
}
