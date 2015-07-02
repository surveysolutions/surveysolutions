using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Memory.Implementation
{
    internal class MemoryCachedReadSideStore<TEntity> : IReadSideStorage<TEntity>, ICacheableRepositoryWriter, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideStorage<TEntity> readSideStorage;
        private readonly ReadSideStoreMemoryCacheSettings settings;

        private bool isCacheEnabled = false;

        protected readonly Dictionary<string, TEntity> cache = new Dictionary<string, TEntity>();

        public MemoryCachedReadSideStore(IReadSideStorage<TEntity> readSideStorage, ReadSideStoreMemoryCacheSettings settings)
        {
            this.readSideStorage = readSideStorage;
            this.settings = settings;
        }

        public void EnableCache()
        {
            this.isCacheEnabled = true;
        }

        public void DisableCache()
        {
            while (cache.Any())
            {
                this.StoreBulkEntitiesToRepository(cache.Keys.ToList());
            }

            this.isCacheEnabled = false;
        }

        public string GetReadableStatus()
        {
            int cachedEntities = this.cache.Count;
            return string.Format("{0}  |  cache {1}  |  cached {2}",
                this.readSideStorage.GetReadableStatus(),
                this.isCacheEnabled ? "enabled" : "disabled",
                cachedEntities);
        }

        public Type ViewType
        {
            get { return typeof(TEntity); }
        }

        public bool IsCacheEnabled { get { return this.isCacheEnabled; } }

        public void Clear()
        {
            var readSideRepositoryCleaner = readSideStorage as IReadSideRepositoryCleaner;
            if(readSideRepositoryCleaner!=null)
                readSideRepositoryCleaner.Clear();
        }

        public TEntity GetById(string id)
        {
            return this.isCacheEnabled
                ? this.GetByIdUsingCache(id)
                : readSideStorage.GetById(id);
        }

        public void Remove(string id)
        {
            if (this.isCacheEnabled)
            {
                this.RemoveUsingCache(id);
            }
            else
            {
                readSideStorage.Remove(id);
            }
        }

        public void Store(TEntity view, string id)
        {
            if (this.isCacheEnabled)
            {
                this.StoreUsingCache(view, id);
            }
            else
            {
                readSideStorage.Store(view, id);
            }
        }

        private TEntity GetByIdUsingCache(string id)
        {
            if (cache.ContainsKey(id))
                return cache[id];

            var entity = readSideStorage.GetById(id);

            cache[id] = entity;

            this.ReduceCacheIfNeeded();

            return entity;
        }

        private void RemoveUsingCache(string id)
        {
            this.cache.Remove(id);
            this.readSideStorage.Remove(id);
        }

        private void StoreUsingCache(TEntity entity, string id)
        {
            this.cache[id] = entity;

            this.ReduceCacheIfNeeded();
        }

        private void ReduceCacheIfNeeded()
        {
            if (this.IsCacheLimitReached())
            {
                this.ReduceCache();
            }
        }

        private void ReduceCache()
        {
            var bulk = this.cache.Keys.Take(this.settings.MaxCountOfEntitiesInOneStoreOperation).ToList();
            this.StoreBulkEntitiesToRepository(bulk);
        }

        protected virtual void StoreBulkEntitiesToRepository(IEnumerable<string> bulk)
        {
            foreach (var entityId in bulk)
            {
                var entity = cache[entityId];
                if (entity == null)
                {
                    readSideStorage.Remove(entityId);
                }
                else
                {
                    readSideStorage.Store(entity, entityId);
                }

                cache.Remove(entityId);
            }
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= this.settings.MaxCountOfCachedEntities;
        }
    }
}
