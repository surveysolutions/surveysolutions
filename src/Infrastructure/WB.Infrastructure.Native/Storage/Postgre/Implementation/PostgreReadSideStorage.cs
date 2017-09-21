﻿using System;
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
    internal class PostgreReadSideStorage<TEntity> : IReadSideRepositoryWriter<TEntity>,
        IReadSideRepositoryCleaner,
        INativeReadSideStorage<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly ISessionProvider sessionProvider;
        private readonly ILogger logger;
        private readonly string entityIdentifierColumnName;

        public PostgreReadSideStorage([Named(PostgresReadSideModule.SessionProviderName)]ISessionProvider sessionProvider, ILogger logger, string entityIdentifierColumnName)
        {
            this.sessionProvider = sessionProvider;
            this.logger = logger;
            this.entityIdentifierColumnName = entityIdentifierColumnName;
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
        
        public void RemoveIfStartsWith(string beginingOfId)
        {
            var session = this.sessionProvider.GetSession();

            string hql = $"DELETE {typeof(TEntity).Name} e WHERE e.{entityIdentifierColumnName} like :id";

            session.CreateQuery(hql).SetParameter("id", $"{beginingOfId}%").ExecuteUpdate();
        }

        public IEnumerable<string> GetIdsStartWith(string beginingOfId)
        {
            var session = this.sessionProvider.GetSession();

            string hql = $"SELECT e.{entityIdentifierColumnName} FROM {typeof(TEntity).Name} e WHERE e.{entityIdentifierColumnName} like :id";

            return session.CreateQuery(hql).SetParameter("id", $"{beginingOfId}%").List<string>().ToList();
        }

        public virtual void Store(TEntity entity, string id)
        {
            ISession session = this.sessionProvider.GetSession();

            var storedEntity = session.Get<TEntity>(id);
            if (!object.ReferenceEquals(storedEntity, entity) && storedEntity != null)
            {
                session.Merge(storedEntity);
            }
            else
            {
                session.SaveOrUpdate(entity);
            }
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

        public int CountDistinctWithRecursiveIndex<TResult>(Func<IQueryOver<TEntity, TEntity>, IQueryOver<TResult,TResult>> query)
        {
            var queryable= query.Invoke(this.sessionProvider.GetSession().QueryOver<TEntity>());

            var countQuery = this.GenerateCountRowsQuery(queryable.UnderlyingCriteria);

            var result = countQuery.UniqueResult<long>();

            return (int)result;
        }

        public IQuery GenerateCountRowsQuery(ICriteria criteria)
        {
            ISession session = this.sessionProvider.GetSession();
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
            return query.Invoke(this.sessionProvider.GetSession().QueryOver<TEntity>());
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