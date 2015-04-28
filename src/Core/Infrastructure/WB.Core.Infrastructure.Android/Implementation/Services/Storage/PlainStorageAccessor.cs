using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sqo;
using Sqo.Transactions;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Storage
{
    internal class PlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity> where TEntity: class, IPlainStorageEntity
    {
        protected readonly ISiaqodb Storage;

        public PlainStorageAccessor(ISiaqodb storage)
        {
            this.Storage = storage;
        }

        public Task<TEntity> GetByIdAsync(string id)
        {
            return Task.Run(() => this.Storage.Query<TEntity>().FirstOrDefault(_ => _.Id == id));
        }

        public async Task RemoveAsync(string id)
        {
            TEntity entity = await this.GetByIdAsync(id);

            await this.RemoveAsync(new[] { entity });
        }

        public async Task RemoveAsync(IEnumerable<TEntity> entities)
        {
            ITransaction transaction = this.Storage.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    await this.Storage.DeleteAsync(entity, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
        }

        public async Task StoreAsync(TEntity entity)
        {
            await this.StoreAsync(new[] { entity });
        }

        public async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            ITransaction transaction = this.Storage.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    await this.Storage.StoreObjectAsync(entity, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
        }

        public Task<TResult> QueryAsync<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            return Task.Run(() =>
            {
                var queryable = this.Storage.Cast<TEntity>().AsQueryable();
                var result = query.Invoke(queryable);
                return result;
            });
        }
    }
}