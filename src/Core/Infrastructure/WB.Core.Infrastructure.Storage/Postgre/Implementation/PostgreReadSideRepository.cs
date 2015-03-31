using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Practices.ServiceLocation;
using NHibernate;
using NHibernate.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgreReadSideRepository<TEntity> : IReadSideRepositoryWriter<TEntity>,
        IReadSideRepositoryCleaner,
        IQueryableReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly ISessionProvider sessionProvider;

        public PostgreReadSideRepository(ISessionProvider sessionProvider)
        {
            this.sessionProvider = sessionProvider;
        }

        public int Count()
        {
            return this.sessionProvider.GetSession().QueryOver<TEntity>().RowCount();
        }

        public TEntity GetById(string id)
        {
            return this.sessionProvider.GetSession().Get<TEntity>(id);
        }

        public void Remove(string id)
        {
            var session = this.sessionProvider.GetSession();

            var entity = session.Get<TEntity>(id);

            if (entity == null)
                return;

            session.Delete(entity);
        }

        public void Store(TEntity entity, string id)
        {
            ISession session = this.sessionProvider.GetSession();

            var storedEntity = session.Get<TEntity>(id);

            if (!object.ReferenceEquals(storedEntity, entity) && storedEntity != null)
            {
                session.Evict(storedEntity);
            }

            session.SaveOrUpdate(null, entity, id);
        }

        public void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            var sessionFactory = ServiceLocator.Current.GetInstance<ISessionFactory>(PostgresReadSideModule.ReadSideSessionFactoryName);
            ISession session = sessionFactory.OpenSession();
            using(var transaction = session.BeginTransaction())
            {
                foreach (var tuple in bulk)
                {
                    TEntity entity = tuple.Item1;
                    var merge = session.Merge(entity);
                    session.SaveOrUpdate(merge);
                }

                transaction.Commit();
            }
        }

        public void Clear()
        {
            ISession session = this.sessionProvider.GetSession();

            string entityName = typeof(TEntity).Name;

            session.Delete(string.Format("from {0} e", entityName));
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return query.Invoke(this.sessionProvider.GetSession().Query<TEntity>());
        }

        public IEnumerable<TEntity> QueryAll(Expression<Func<TEntity, bool>> condition = null)
        {
            return this.sessionProvider.GetSession().Query<TEntity>().Where(condition).ToList();
        }

        public Type ViewType
        {
            get { return typeof(TEntity); }
        }

        public string GetReadableStatus()
        {
            return "PostgreSQL :'(";
        }
    }
}