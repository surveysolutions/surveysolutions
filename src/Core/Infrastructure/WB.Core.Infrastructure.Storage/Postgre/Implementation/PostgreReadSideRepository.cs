using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using NHibernate;
using NHibernate.Linq;
using Ninject;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ILogger logger;

        public PostgreReadSideRepository([Named(PostgresReadSideModule.SessionProviderName)]ISessionProvider sessionProvider, ILogger logger)
        {
            this.sessionProvider = sessionProvider;
            this.logger = logger;
        }

        public virtual int Count()
        {
            return this.sessionProvider.GetSession().QueryOver<TEntity>().RowCount();
        }

        public virtual TEntity GetById(string id)
        {
            return this.sessionProvider.GetSession().Get<TEntity>(id);
        }

        public virtual void Remove(string id)
        {
            var session = this.sessionProvider.GetSession();

            var entity = session.Get<TEntity>(id);

            if (entity == null)
                return;

            session.Delete(entity);
        }

        public virtual void Store(TEntity entity, string id)
        {
            ISession session = this.sessionProvider.GetSession();

            var storedEntity = session.Get<TEntity>(id);

            if (!object.ReferenceEquals(storedEntity, entity) && storedEntity != null)
            {
                session.Evict(storedEntity);
            }

            session.SaveOrUpdate(null, entity, id);
        }

        public virtual void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            try
            {
                this.FastBulkStore(bulk);
            }
            catch (Exception exception)
            {
                this.logger.Warn($"Failed to store bulk of {bulk.Count} entities of type {this.ViewType.Name} using fast way. Switching to slow way.", exception);

                this.SlowBulkStore(bulk);
            }
        }

        public virtual void Clear()
        {
            ISession session = this.sessionProvider.GetSession();

            string entityName = typeof(TEntity).Name;

            session.Delete(string.Format("from {0} e", entityName));
        }

        public virtual TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return query.Invoke(this.sessionProvider.GetSession().Query<TEntity>());
        }

        public Type ViewType
        {
            get { return typeof(TEntity); }
        }

        public string GetReadableStatus()
        {
            return "PostgreSQL :'(";
        }

        private void FastBulkStore(List<Tuple<TEntity, string>> bulk)
        {
            var sessionFactory = ServiceLocator.Current.GetInstance<ISessionFactory>(PostgresReadSideModule.ReadSideSessionFactoryName);

            foreach (var subBulk in bulk.Batch(2048))
            {
                using (ISession session = sessionFactory.OpenSession())
                using (ITransaction transaction = session.BeginTransaction())
                {
                    foreach (var tuple in subBulk)
                    {
                        TEntity entity = tuple.Item1;
                        string id = tuple.Item2;

                        session.Save(entity, id);
                    }

                    transaction.Commit();
                }
            }
        }

        private void SlowBulkStore(List<Tuple<TEntity, string>> bulk)
        {
            var sessionFactory = ServiceLocator.Current.GetInstance<ISessionFactory>(PostgresReadSideModule.ReadSideSessionFactoryName);
            using (ISession session = sessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                foreach (var tuple in bulk)
                {
                    TEntity entity = tuple.Item1;
                    string id = tuple.Item2;

                    var storedEntity = session.Get<TEntity>(id);

                    if (storedEntity != null)
                    {
                        var merge = session.Merge(entity);
                        session.Update(merge);
                    }
                    else
                    {
                        session.Save(entity);
                    }
                }

                transaction.Commit();
            }
        }
    }
}