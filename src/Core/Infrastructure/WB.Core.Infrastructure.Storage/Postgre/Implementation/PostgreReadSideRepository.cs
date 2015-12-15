﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using NHibernate;
using NHibernate.Linq;
using Ninject;
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

        public PostgreReadSideRepository([Named(PostgresReadSideModule.SessionProviderName)]ISessionProvider sessionProvider)
        {
            this.sessionProvider = sessionProvider;
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
    }
}