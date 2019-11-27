﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Threading;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Impl;
using NHibernate.Loader.Criteria;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Persister.Entity;
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
            ILogger logger,
            IServiceLocator serviceLocator) : base(unitOfWork, logger, serviceLocator)
        {
        }
    }

    internal class PostgreReadSideStorage<TEntity, TKey> : IReadSideRepositoryWriter<TEntity, TKey>,
        INativeReadSideStorage<TEntity, TKey>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger logger;
        private readonly IServiceLocator serviceLocator;

        public PostgreReadSideStorage(IUnitOfWork unitOfWork,
            ILogger logger,
            IServiceLocator serviceLocator)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.serviceLocator = serviceLocator;
        }

        public virtual int Count()
        {
            return this.unitOfWork.Session.QueryOver<TEntity>().RowCount();
        }

        static readonly MemoryCache Cache = new MemoryCache("Map key alias to id cache");

        public virtual TEntity GetById(TKey id)
        {
            if (ReadSideStorageMapping.IsPrimaryKeyAlias<TEntity, TKey>())
            {
                var cacheKey = id.ToString();
                var primaryKey = Cache.Get(cacheKey);

                if (primaryKey != null)
                {
                    // using cached primaryKey to make use of NHibernate first-level cache
                    return this.unitOfWork.Session.Get<TEntity>(primaryKey);
                }

                var item = this.unitOfWork.Session.Query<TEntity>().GetByPrimaryKeyAlias(id);
                if (item == null) return item;

                // getting primary key value to add to cache
                primaryKey = this.unitOfWork.Session.SessionFactory.GetClassMetadata(typeof(TEntity)).GetIdentifier(item);

                Cache.Add(cacheKey, primaryKey, new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });

                return item;
            }

            return this.unitOfWork.Session.Get<TEntity>(id);
        }

        public virtual void Remove(TKey id)
        {
            var session = this.unitOfWork.Session;

            var entity = GetById(id);

            if (entity == null)
                return;

            session.Delete(entity);
        }

        public virtual void Store(TEntity entity, TKey id)
        {
            ISession session = this.unitOfWork.Session;

            if (session.Contains(entity))
            {
                return;
            }

            var storedEntity = GetById(id);
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
            this.unitOfWork.Session.Flush();
        }

        public int CountDistinctWithRecursiveIndex<TResult>(Func<IQueryOver<TEntity, TEntity>, IQueryOver<TResult, TResult>> query)
        {
            var queryable = query.Invoke(this.unitOfWork.Session.QueryOver<TEntity>());

            var countQuery = this.GenerateCountRowsQuery(queryable.UnderlyingCriteria);

            var result = countQuery.UniqueResult<long>();

            return (int)result;
        }

        public IQuery GenerateCountRowsQuery(ICriteria criteria)
        {
            ISession session = this.unitOfWork.Session;

            var criteriaImpl = (CriteriaImpl)criteria;
            var sessionImpl = (SessionImpl)criteriaImpl.Session;
            var factory = (SessionFactoryImpl)sessionImpl.SessionFactory;
            var implementors = factory.GetImplementors(criteriaImpl.EntityOrClassName);
            var loader = new CriteriaLoader((IOuterJoinLoadable)factory.GetEntityPersister(implementors[0]), factory, criteriaImpl, implementors[0], sessionImpl.EnabledFilters);

            if (loader.Translator.ProjectedColumnAliases.Length != 1)
            {
                throw new InvalidOperationException("Recursive index is available only for single column query");
            }

            var aliasName = loader.Translator.ProjectedColumnAliases[0];
            var propertyProjection = (PropertyProjection)criteriaImpl.Projection;
            var columnName = propertyProjection.PropertyName;

            var result = session.CreateSQLQuery($"WITH RECURSIVE t AS ( ({loader.SqlString} ORDER BY {columnName} LIMIT 1) " +
                                                $"UNION ALL SELECT({loader.SqlString} and {columnName} > t.{aliasName} ORDER BY {columnName} LIMIT 1) FROM t WHERE t.{aliasName} IS NOT NULL)" +
                                                $"SELECT count(*) FROM t WHERE {aliasName} IS NOT NULL; ");
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
            return query.Invoke(this.unitOfWork.Session.QueryOver<TEntity>());
        }

        public virtual TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return query.Invoke(this.unitOfWork.Session.Query<TEntity>());
        }

        public Type ViewType => typeof(TEntity);

        public string GetReadableStatus()
        {
            return "PostgreSQL :'(";
        }

        private void FastBulkStore(List<Tuple<TEntity, TKey>> bulk)
        {
            var sessionFactory = serviceLocator.GetInstance<ISessionFactory>();

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
            var sessionFactory = serviceLocator.GetInstance<ISessionFactory>();
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

    public static class ReadSideStorageMapping
    {
        static readonly Dictionary<(Type, Type), object> aliasGetters = new Dictionary<(Type, Type), object>();

        public static void PropertyKeyAlias<TKey, TEntity>(this ClassMapping<TEntity> map,
            Expression<Func<TEntity, TKey>> property, Func<TKey, Expression<Func<TEntity, bool>>> getter) where TEntity : class
        {
            var memberInfo = NHibernate.Mapping.ByCode.TypeExtensions.DecodeMemberAccessExpressionOf(property);
            map.Property(property);

            var key = (memberInfo.DeclaringType, memberInfo.GetPropertyOrFieldType());

            if (!aliasGetters.ContainsKey(key))
            {
                aliasGetters.Add(key, getter);
            }
        }

        public static bool IsPrimaryKeyAlias<TEntity, TKey>()
        {
            return aliasGetters.ContainsKey((typeof(TEntity), typeof(TKey)));
        }

        public static TEntity GetByPrimaryKeyAlias<TEntity, TKey>(this IQueryable<TEntity> query, TKey id)
            where TEntity : class
        {
            if (aliasGetters.TryGetValue((typeof(TEntity), typeof(TKey)), out var prop))
            {
                var property = (Func<TKey, Expression<Func<TEntity, bool>>>)prop;
                return query.SingleOrDefault(property(id));
            }

            throw new NotSupportedException();
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAliasAttribute : Attribute
    {
    }
}
