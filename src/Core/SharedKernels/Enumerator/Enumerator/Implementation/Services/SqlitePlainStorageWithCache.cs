using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sqo;
using SQLite.Net.Interop;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqlitePlainStorageWithCache<TEntity> : SqlitePlainStorage<TEntity> where TEntity : class, IPlainStorageEntity
    {
        readonly Dictionary<string, TEntity> memoryStorage = new Dictionary<string, TEntity>();

        public SqlitePlainStorageWithCache(ISQLitePlatform sqLitePlatform, ILogger logger,
            IAsynchronousFileSystemAccessor fileSystemAccessor, ISerializer serializer, SqliteSettings settings)
            : base(sqLitePlatform, logger, fileSystemAccessor, serializer, settings)
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
            await base.RemoveAsync(entitiesToRemove);
            this.RemoveFromMemory(entitiesToRemove);
        }

        public override async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            try
            {
                await base.StoreAsync(entities);
                foreach (var entity in entities.Where(entity => entity != null))
                {
                    this.StoreToMemory(entity);
                }
            }
            catch (Exception ex)
            {
                this.logger.Fatal(ex.Message, ex);
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