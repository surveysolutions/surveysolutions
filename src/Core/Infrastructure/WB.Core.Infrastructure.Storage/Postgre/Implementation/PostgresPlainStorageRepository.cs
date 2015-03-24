using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    public class PostgresPlainStorageRepository<TEntity> : IQueryablePlainStorageAccessor<TEntity> where TEntity : class
    {
        public TEntity GetById(string id)
        {
            throw new NotImplementedException();
        }

        public void Remove(string id)
        {
            throw new NotImplementedException();
        }

        public void Store(TEntity entity, string id)
        {
            throw new NotImplementedException();
        }

        public void Store(IEnumerable<Tuple<TEntity, string>> entities)
        {
            throw new NotImplementedException();
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            throw new NotImplementedException();
        }
    }
}