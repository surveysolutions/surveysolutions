using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgresPlainStorageRepository<TEntity> : IPlainStorageAccessor<TEntity> where TEntity : class
    {
        private readonly IUnitOfWork unitOfWork;

        public PostgresPlainStorageRepository(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public TEntity GetById(object id)
        {
            return this.GetSession().Get<TEntity>(id);
        }

        public void Remove(object id)
        {
            var session = this.GetSession();

            var entity = session.Get<TEntity>(id);

            if (entity == null)
                return;

            session.Delete(entity);
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.GetSession().Delete(entity);
            }
        }

        public void Store(TEntity entity, object id)
        {
            this.GetSession().SaveOrUpdate(entity);
        }

        public void Store(IEnumerable<Tuple<TEntity, object>> entities)
        {
            foreach (var entity in entities)
            {
                this.Store(entity.Item1, entity.Item2);
            }
        }

        public void Flush()
        {
            this.GetSession().Flush();
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return query.Invoke(GetSession().Query<TEntity>());
        }

        public ISession GetSession()
        {
            return this.unitOfWork.Session;
        }
    }
}
