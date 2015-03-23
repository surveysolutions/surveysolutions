using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IPlainStorageAccessor<TEntity>
        where TEntity : class
    {
        TEntity GetById(string id);

        void Remove(string id);
        void Remove(IEnumerable<TEntity> entities);

        void Store(TEntity entity, string id);

        void Store(IEnumerable<Tuple<TEntity, string>> entities);
    }
}