using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Impl;
using NHibernate.Linq;
using NHibernate.Loader.Criteria;
using NHibernate.Persister.Entity;
using Ninject;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgreReadSideStorage<TEntity> : PostgreReadSideStorage<TEntity, string>, 
            IReadSideRepositoryWriter<TEntity>,
            INativeReadSideStorage<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        public PostgreReadSideStorage(
            IUnitOfWork unitOfWork, 
            ILogger logger) : base(unitOfWork, logger)
        {
        }
    }

    internal class PostgreReadSideStorage<TEntity, TKey> : IReadSideRepositoryWriter<TEntity, TKey>,
        INativeReadSideStorage<TEntity,TKey>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IUnitOfWork sessionProvider;
        private readonly ILogger logger;

        public PostgreReadSideStorage(IUnitOfWork unitOfWork, 
            ILogger logger)
        {
            this.sessionProvider = unitOfWork;
            this.logger = logger;
        }

        public virtual int Count()
        {
            return this.sessionProvider.Session.QueryOver<TEntity>().RowCount();
        }

        public virtual TEntity GetById(TKey id)
        {
            return this.sessionProvider.Session.Get<TEntity>(id);
        }

        public virtual void Remove(TKey id)
        {
            var session = this.sessionProvider.Session;

            var entity = session.Get<TEntity>(id);

            if (entity == null)
                return;

            session.Delete(entity);
        }

        public virtual void Store(TEntity entity, TKey id)
        {
            ISession session = this.sessionProvider.Session;

            var storedEntity = session.Get<TEntity>(id);
            if (!object.ReferenceEquals(storedEntity, entity) && storedEntity != null)
            {
                session.Merge(entity);
            }
            else
            {
                session.SaveOrUpdate(entity);
            }
        }

        public virtual void BulkStore(List<Tuple<TEntity, TKey>> bulk)
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

        public void Flush()
        {
            this.sessionProvider.Session.Flush();
        }

        public int CountDistinctWithRecursiveIndex<TResult>(Func<IQueryOver<TEntity, TEntity>, IQueryOver<TResult,TResult>> query)
        {
            var queryable= query.Invoke(this.sessionProvider.Session.QueryOver<TEntity>());

            var countQuery = this.GenerateCountRowsQuery(queryable.UnderlyingCriteria);

            var result = countQuery.UniqueResult<long>();

            return (int)result;
        }

        public IQuery GenerateCountRowsQuery(ICriteria criteria)
        {
            ISession session = this.sessionProvider.Session;
            var criteriaImpl = (CriteriaImpl)criteria;
            var sessionImpl = (SessionImpl)criteriaImpl.Session;
            var factory = (SessionFactoryImpl)sessionImpl.SessionFactory;
            var implementors = factory.GetImplementors(criteriaImpl.EntityOrClassName);
            var loader = new CriteriaLoader((IOuterJoinLoadable)factory.GetEntityPersister(implementors[0]), factory, criteriaImpl, implementors[0], sessionImpl.EnabledFilters);

            if (loader.Translator.ProjectedColumnAliases.Length != 1)
            {
                throw new InvalidOperationException("Recursive index is avalible only for single coulmn query");
            }

            var alliasName = loader.Translator.ProjectedColumnAliases[0];
            var propertyProjection = (PropertyProjection)criteriaImpl.Projection;
            var columnName = propertyProjection.PropertyName;

            var result = session.CreateSQLQuery($"WITH RECURSIVE t AS ( ({loader.SqlString} ORDER BY {columnName} LIMIT 1) " +
                                                $"UNION ALL SELECT({loader.SqlString} and {columnName} > t.{alliasName} ORDER BY {columnName} LIMIT 1) FROM t WHERE t.{alliasName} IS NOT NULL)" +
                                                $"SELECT count(*) FROM t WHERE {alliasName} IS NOT NULL; ");
            int position = 0;
            foreach (var collectedParameter in loader.Translator.CollectedParameters)
            {
                result.SetParameter(position, collectedParameter.Value, collectedParameter.Type);
                position++;
            }
            foreach (var collectedParameter in loader.Translator.CollectedParameters)
            {
                result.SetParameter(position, collectedParameter.Value, collectedParameter.Type);
                position++;
            }
            return result;
        }

        public virtual TResult QueryOver<TResult>(Func<IQueryOver<TEntity, TEntity>, TResult> query)
        {
            return query.Invoke(this.sessionProvider.Session.QueryOver<TEntity>());
        }

        public virtual TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return query.Invoke(this.sessionProvider.Session.Query<TEntity>());
        }

        public Type ViewType
        {
            get { return typeof(TEntity); }
        }

        public string GetReadableStatus()
        {
            return "PostgreSQL :'(";
        }

        private void FastBulkStore(List<Tuple<TEntity, TKey>> bulk)
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
                        TKey id = tuple.Item2;

                        session.Save(entity, id);
                    }

                    transaction.Commit();
                }
            }
        }

        private void SlowBulkStore(List<Tuple<TEntity, TKey>> bulk)
        {
            var sessionFactory = ServiceLocator.Current.GetInstance<ISessionFactory>(PostgresReadSideModule.ReadSideSessionFactoryName);
            using (ISession session = sessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                foreach (var tuple in bulk)
                {
                    TEntity entity = tuple.Item1;
                    TKey id = tuple.Item2;

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
