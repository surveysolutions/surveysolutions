using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using Ninject;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    public class PostgresPlainStorageRepository<TEntity> : IPlainStorageAccessor<TEntity> where TEntity : class
    {
        private readonly ISession session;

        public PostgresPlainStorageRepository([Named(PostgresPlainStorageModule.SessionName)]ISession session)
        {
            if (session == null) throw new ArgumentNullException("session");
            this.session = session;
        }

        public TEntity GetById(object id)
        {
            return this.session.Get<TEntity>(id);
        }

        public void Remove(object id)
        {
            this.session.Delete(id);
        }

        public void Store(TEntity entity, object id)
        {
            this.session.SaveOrUpdate(entity);
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
            return query.Invoke(this.session.Query<TEntity>());
        }
    }
}