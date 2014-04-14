using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WB.Core.Infrastructure.PlainStorage;

namespace AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage
{
    public class SqlitePlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity>
        where TEntity : PlainStorageRow, new()
    {
        private readonly SqlitePlainStore documentStore;
        public SqlitePlainStorageAccessor(SqlitePlainStore documentStore)
        {
            this.documentStore = documentStore;
        }

        public TEntity GetById(string id)
        {
            return this.documentStore.GetById<TEntity>(id);
        }

        public void Remove(string id)
        {
            this.documentStore.Remove<TEntity>(id);
        }

        public void Store(TEntity entity, string id)
        {
            this.documentStore.Store<TEntity>(entity, id);
        }
    }
}