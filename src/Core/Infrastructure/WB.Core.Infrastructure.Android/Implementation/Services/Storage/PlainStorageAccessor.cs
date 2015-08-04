using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sqo;
using Sqo.Transactions;
using WB.Core.BoundedContexts.Tester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Storage
{
    internal class PlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity> where TEntity: class, IPlainStorageEntity
    {
        protected readonly ISiaqodb Storage;

        public PlainStorageAccessor(ISiaqodb storage)
        {
            this.Storage = storage;
        }

        public TEntity GetById(string id)
        {
            return this.Storage.Query<TEntity>().FirstOrDefault(_ => _.Id == id);
        }

        public async Task RemoveAsync(string id)
        {
            TEntity entity = this.GetById(id);

            await this.RemoveAsync(new[] { entity });
        }

        public async Task RemoveAsync(IEnumerable<TEntity> entities)
        {
            var isFailedTransaction = false;

            ITransaction transaction = this.Storage.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    await this.Storage.DeleteAsync(entity, transaction);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                isFailedTransaction = true;
            }

            if (isFailedTransaction)
                await transaction.RollbackAsync();
        }

        public async Task StoreAsync(TEntity entity)
        {
            await this.StoreAsync(new[] { entity });
        }

        public async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            var isFailedTransaction = false;

            ITransaction transaction = this.Storage.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    await this.Storage.StoreObjectAsync(entity, transaction);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                isFailedTransaction = true;
            }

            if(isFailedTransaction)
                await transaction.RollbackAsync();
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            var queryable = this.Storage.Cast<TEntity>().AsQueryable();
            var result = query.Invoke(queryable);
            return result;
        }
    }
}