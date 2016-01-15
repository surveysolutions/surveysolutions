using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sqo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SiaqodbPlainStorageWithCache<TEntity> : SiaqodbPlainStorage<TEntity> where TEntity: class, IPlainStorageEntity
    {
        private readonly Dictionary<string, TEntity> memoryStorage = new Dictionary<string, TEntity>();

        public SiaqodbPlainStorageWithCache(Siaqodb storage, IUserInteractionService userInteractionService, ILogger logger)
            : base(storage, userInteractionService, logger) {}

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
            await base.RemoveAsync(entities);

            this.RemoveFromMemory(entities);
        }

        public override async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            await base.StoreAsync(entities);

            foreach (var entity in entities.Where(entity => entity != null))
            {
                this.memoryStorage[entity.Id] = entity;
            }
        }

        private void RemoveFromMemory(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities.Where(entity => entity != null))
            {
                if (this.memoryStorage.ContainsKey(entity.Id))
                {
                    this.memoryStorage.Remove(entity.Id);
                }
            }
        }
    }
}