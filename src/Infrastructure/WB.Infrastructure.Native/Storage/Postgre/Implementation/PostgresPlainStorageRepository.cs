using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgresPlainStorageRepository<TEntity> 
        : IPlainStorageAccessor<TEntity> where TEntity : class
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

        public Task<TEntity> GetByIdAsync(object id)
        {
            return this.GetSession().GetAsync<TEntity>(id);
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

        public void Remove(Func<IQueryable<TEntity>, IQueryable<TEntity>> query) => query.Invoke(GetSession().Query<TEntity>()).Delete();

        public void Store(TEntity entity, object id) => this.Store(new[] {entity});
        public void Store(IEnumerable<Tuple<TEntity, object>> entities) => this.Store(entities.Select(x => x.Item1));
        public void Store(IEnumerable<TEntity> entities) => entities.ForEach(this.GetSession().SaveOrUpdate);

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
