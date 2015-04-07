using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using Ninject;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgresPlainStorageRepository<TEntity> : IPlainStorageAccessor<TEntity> where TEntity : class
    {
        private readonly ISessionProvider sessionProvider;

        public PostgresPlainStorageRepository(IPlainSessionProvider sessionProvider)
        {
            this.sessionProvider = sessionProvider;
        }

        public TEntity GetById(object id)
        {
            return this.sessionProvider.GetSession().Get<TEntity>(id);
        }

        public void Remove(object id)
        {
            this.sessionProvider.GetSession().Delete(id);
        }

        public void Store(TEntity entity, object id)
        {
            this.sessionProvider.GetSession().SaveOrUpdate(entity);
        }

        public void Store(IEnumerable<Tuple<TEntity, object>> entities)
        {
            foreach (var entity in entities)
            {
                this.Store(entity.Item1, entity.Item2);
            }
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return query.Invoke(this.sessionProvider.GetSession().Query<TEntity>());
        }
    }
}