using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sqo;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SiaqodbPlainStorageWithCache<TEntity> : SiaqodbPlainStorage<TEntity> where TEntity: class, IPlainStorageEntity
    {
        readonly Dictionary<string, TEntity> memoryStorage = new Dictionary<string, TEntity>();

        public SiaqodbPlainStorageWithCache(ISiaqodb storage, IUserInteractionService userInteractionService)
            : base(storage, userInteractionService)
        {
        }

        public override TEntity GetById(string id)
        {
            if (this.memoryStorage.ContainsKey(id)) return this.memoryStorage[id];

            var entity = base.GetById(id);
            if (entity != null)
                this.memoryStorage.Add(id, entity);

            return entity;
        }

        public override async Task RemoveAsync(IEnumerable<TEntity> entities)
        {
            var entitiesToRemove = entities.Where(entity => entity != null).ToList();

            foreach (var entity in entitiesToRemove)
            {
                await this.Storage.DeleteAsync(entity);
            }
            this.RemoveFromMemory(entitiesToRemove);
        }

        public override async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            try
            {
                foreach (var entity in entities.Where(entity => entity != null))
                {
                    await this.Storage.StoreObjectAsync(entity);
                    this.StoreToMemory(entity);
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

        private void StoreToMemory(TEntity entity)
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