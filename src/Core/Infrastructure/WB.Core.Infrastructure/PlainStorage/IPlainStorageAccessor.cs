using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IPlainStorageAccessor<TEntity> where TEntity : class
    {
        TEntity GetById(object id);

        void Remove(object id);
        void Remove(IEnumerable<TEntity> entities);

        void Store(TEntity entity, object id);

        void Store(IEnumerable<Tuple<TEntity, object>> entities);

        TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query);
    }
}