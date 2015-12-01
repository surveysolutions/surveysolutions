using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sqo;
using Sqo.Transactions;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SiaqodbPlainStorageWithCache<TEntity> : SiaqodbPlainStorage<TEntity> where TEntity: class, IPlainStorageEntity
    {
        readonly Dictionary<string, TEntity> memoryStorage = new Dictionary<string, TEntity>(); 
        public SiaqodbPlainStorageWithCache(ISiaqodb storage) : base(storage)
        {
        }

        public override async Task<TEntity> GetByIdAsync(string id)
        {
            if (!this.memoryStorage.ContainsKey(id))
            {
                this.memoryStorage.Add(id, await base.GetByIdAsync(id));
            }

            return this.memoryStorage[id];
        }

        public override async Task RemoveAsync(IEnumerable<TEntity> entities)
        {
            var entitiesToRemove = entities.Where(entity => entity != null).ToList();

            ITransaction transaction = this.Storage.BeginTransaction();
            try
            {
                foreach (var entity in entitiesToRemove)
                {
                    await this.Storage.DeleteAsync(entity, transaction);
                }

                await transaction.CommitAsync();
                this.RemoveFromMemory(entitiesToRemove);
            }
            catch
            {
                await transaction.RollbackAsync();
            }
        }

        public override async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            var entitiesToStore = entities.Where(entity => entity != null).ToList();

            ITransaction transaction = this.Storage.BeginTransaction();
            try
            {
                foreach (var entity in entitiesToStore)
                {
                    await this.Storage.StoreObjectAsync(entity, transaction);
                }

                await transaction.CommitAsync();
                this.StoreToMemory(entitiesToStore);
            }
            catch
            {
                await transaction.RollbackAsync();
            }
        }

        private void StoreToMemory(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (!this.memoryStorage.ContainsKey(entity.Id))
                {
                    this.memoryStorage.Add(entity.Id, entity);
                }
                else
                {
                    this.memoryStorage[entity.Id] = entity;
                }
            }
        }

        private void RemoveFromMemory(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (this.memoryStorage.ContainsKey(entity.Id))
                    this.memoryStorage.Remove(entity.Id);
            }
        }
    }
}