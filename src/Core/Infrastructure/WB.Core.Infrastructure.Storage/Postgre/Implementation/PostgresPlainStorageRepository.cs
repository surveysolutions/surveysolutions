﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
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
            var session = this.sessionProvider.GetSession();

            var entity = session.Get<TEntity>(id);

            if (entity == null)
                return;

            session.Delete(entity);
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.sessionProvider.GetSession().Delete(entity);
            }
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