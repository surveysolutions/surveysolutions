using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Memory.Implementation
{
    internal class MemoryCachedReadSideRepositoryWriter<TEntity> : MemoryCachedReadSideStore<TEntity>,  
        IReadSideRepositoryWriter<TEntity> where TEntity : class, IReadSideRepositoryEntity       
    {
        private readonly IReadSideRepositoryWriter<TEntity> writer;

        public MemoryCachedReadSideRepositoryWriter(IReadSideRepositoryWriter<TEntity> readSideStorage)
            : base(readSideStorage)
        {
            this.writer = readSideStorage;
        }

        protected override void StoreBulkEntitiesToRepository(List<string> bulk)
        {
            var entitiesToStore = new List<Tuple<TEntity, string>>();
            foreach (var entityId in bulk)
            {
                var entity = cache[entityId];
                if (entity == null)
                {
                    this.writer.Remove(entityId);
                }
                else
                {
                    entitiesToStore.Add(Tuple.Create(entity, entityId));
                }
                cache.Remove(entityId);
            }

            writer.BulkStore(entitiesToStore);
        }

    }
}
