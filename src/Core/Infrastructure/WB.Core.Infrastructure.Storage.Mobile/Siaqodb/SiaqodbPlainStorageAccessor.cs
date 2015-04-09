using System;
using System.Collections.Generic;
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

        public TEntity GetById(string id)
        {
            return this.Storage.Query<TEntity>().FirstOrDefault(_ => _.Id == id);
        }

        public void Remove(string id)
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

        public void Store(TEntity view, string id)
        {
            this.Store(new[] {new Tuple<TEntity, string>(view, id),});
        }

        public void Store(IEnumerable<Tuple<TEntity, string>> entities)
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
    }
}