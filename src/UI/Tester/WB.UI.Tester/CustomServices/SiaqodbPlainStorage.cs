using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sqo;
using Sqo.Transactions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Tester.CustomServices
{
    public class SiaqodbPlainStorage<TEntity> : IAsyncPlainStorage<TEntity> where TEntity: class, IPlainStorageEntity
    {
        private readonly Siaqodb storage;
        private readonly IUserInteractionService userInteractionService;
        private readonly ILogger logger;
        private static readonly object dbSizeLockObject = new object();

        public SiaqodbPlainStorage(Siaqodb storage, IUserInteractionService userInteractionService, ILogger logger)
        {
            this.storage = storage;
            this.userInteractionService = userInteractionService;
            this.logger = logger;
        }

        public virtual TEntity GetById(string id)
        {
            return this.Query<TEntity>(entities => entities.SingleOrDefault(entity => entity.Id == id));
        }

        public async Task RemoveAsync(string id)
        {
            TEntity entity = this.GetById(id);

            await this.RemoveAsync((IEnumerable<TEntity>) new[] { entity });
        }

        public virtual async Task RemoveAsync(IEnumerable<TEntity> entities)
        {
            ITransaction transaction = this.BeginTransactionRespectingDBSize();
            try
            {
                foreach (var entity in entities.Where(entity => entity != null))
                {
                    await this.storage.DeleteAsync(entity, transaction);
                }
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                this.logger.Fatal(ex.Message, ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task StoreAsync(TEntity entity)
        {
            await this.StoreAsync((IEnumerable<TEntity>) new[] { entity });
        }

        public virtual async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            ITransaction transaction = this.BeginTransactionRespectingDBSize();
            try
            {
                foreach (var entity in entities.Where(entity => entity != null))
                {
                    await this.storage.StoreObjectAsync(entity, transaction);
                }
                await transaction.CommitAsync(); 
            }
            catch (Exception ex)
            {
                this.logger.Fatal(ex.Message, ex);

                await transaction.RollbackAsync();

                if (ex.Message.IndexOf("Environment mapsize limit reached", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    await this.userInteractionService.AlertAsync("Database is full. Please, restart application to increase database size", "Critical exception");
                }
                else
                {
                    throw;
                }
            }
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            var queryable = this.storage.Cast<TEntity>().AsQueryable();
            var result = query.Invoke(queryable);
            return result;
        }

        private ITransaction BeginTransactionRespectingDBSize()
        {
            if (this.ShouldIncreaseDBSize())
            {
                lock (dbSizeLockObject)
                {
                    if (this.ShouldIncreaseDBSize())
                    {
                        var storagePath = this.storage.DbInfo.Path;

                        this.storage.Close();
                        this.storage.Open(storagePath);
                    }
                }
            }

            return this.storage.BeginTransaction();
        }

        private bool ShouldIncreaseDBSize()
        {
            return this.storage.DbInfo.UsedSize > 0.75m * this.storage.DbInfo.MaxSize;
        }
    }
}