using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Sqo;
using Sqo.Transactions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Infrastructure.Storage.Mobile.Siaqodb
{
    public class SiaqodbPlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity> where TEntity: class, IPlainStorageEntity
    {
        protected readonly ISiaqodb Storage;

        public SiaqodbPlainStorageAccessor(ISiaqodb storage)
        {
            this.Storage = storage;
        }

        public TEntity GetById(object id)
        {
            return this.Storage.Query<TEntity>().FirstOrDefault(_ => _.Id == id);
        }

        public void Remove(object id)
        {
            TEntity entity = this.GetById(id);

            this.Remove(new[] {entity});
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            ITransaction transaction = this.Storage.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    this.Storage.Delete(entity, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
        }

        public void Store(TEntity view, object id)
        {
            this.Store(new[] {new Tuple<TEntity, object>(view, id),});
        }

        public void Store(IEnumerable<Tuple<TEntity, object>> entities)
        {
            ITransaction transaction = this.Storage.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    this.Storage.StoreObject(entity, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            var queryable = this.Storage.Cast<TEntity>().AsQueryable();
            var result = query.Invoke(queryable);
            return result;
        }
    }
}