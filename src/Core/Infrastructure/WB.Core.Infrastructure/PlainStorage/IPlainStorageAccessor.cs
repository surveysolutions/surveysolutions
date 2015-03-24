using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IPlainStorageAccessor<TEntity>
        where TEntity : class
    {
        TEntity GetById(object id);

        void Remove(object id);

        void Store(TEntity entity, object id);

        void Store(IEnumerable<Tuple<TEntity, object>> entities);
    }
}