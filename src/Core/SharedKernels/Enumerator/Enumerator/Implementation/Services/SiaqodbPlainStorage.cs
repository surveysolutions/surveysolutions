using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sqo;
using Sqo.Transactions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SiaqodbPlainStorage<TEntity> : IAsyncPlainStorage<TEntity> where TEntity: class, IPlainStorageEntity
    {
        private readonly ISiaqodb storage;
        private readonly IUserInteractionService userInteractionService;
        private readonly ILogger logger;

        public SiaqodbPlainStorage(ISiaqodb storage, IUserInteractionService userInteractionService, ILogger logger)
        {
            this.storage = storage;
            this.userInteractionService = userInteractionService;
            this.logger = logger;
        }

        public virtual TEntity GetById(string id)
        {
            return this.Query(entities => entities.FirstOrDefault(entity => entity.Id == id));
        }

        public async Task RemoveAsync(string id)
        {
            TEntity entity = await Task.FromResult(this.GetById(id));

            await this.RemoveAsync(new[] { entity });
        }

        public virtual async Task RemoveAsync(IEnumerable<TEntity> entities)
        {
            ITransaction transaction = this.storage.BeginTransaction();
            try
            {
                foreach (var entity in entities.Where(entity => entity != null))
                {
                    await this.storage.DeleteAsync(entity, transaction);
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                this.logger.Fatal(ex.Message, ex);
                transaction.Rollback();
            }
        }

        public async Task StoreAsync(TEntity entity)
        {
            await this.StoreAsync(new[] { entity });
        }

        public virtual async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            ITransaction transaction = this.storage.BeginTransaction();
            try
            {
                foreach (var entity in entities.Where(entity => entity != null))
                {
                    await this.storage.StoreObjectAsync(entity, transaction);
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                this.logger.Fatal(ex.Message, ex);
                if (ex.Message.IndexOf("Environment mapsize limit reached", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    await this.userInteractionService.AlertAsync("Database is full. Please, restart application to increase database size", "Critical exception");
                }
                transaction.Rollback();
            }
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            var queryable = this.storage.Cast<TEntity>().AsQueryable();
            var result = query.Invoke(queryable);
            return result;
        }
    }
}