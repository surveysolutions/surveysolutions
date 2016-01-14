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
        protected readonly ISiaqodb Storage;
        protected readonly IUserInteractionService UserInteractionService;
        protected readonly ILogger Logger;

        public SiaqodbPlainStorage(ISiaqodb storage, IUserInteractionService userInteractionService, ILogger logger)
        {
            this.Storage = storage;
            this.UserInteractionService = userInteractionService;
            this.Logger = logger;
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
            foreach (var entity in entities.Where(entity => entity != null))
            {
                await this.Storage.DeleteAsync(entity);
            }
        }

        public async Task StoreAsync(TEntity entity)
        {
            await this.StoreAsync(new[] { entity });
        }

        public virtual async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            try
            {
                foreach (var entity in entities.Where(entity => entity != null))
                {
                    await this.Storage.StoreObjectAsync(entity);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Fatal(ex.Message, ex);
                if (ex.Message.IndexOf("Environment mapsize limit reached", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    await this.UserInteractionService.AlertAsync("Database is full. Please, send tablet information and contact to Survey Solutions team.", "Critical exception");
                }
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