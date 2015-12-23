using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Memory.Implementation
{
    internal class MemoryCachedReadSideStorage<TEntity> : IReadSideRepositoryWriter<TEntity>, IReadSideKeyValueStorage<TEntity>, ICacheableRepositoryWriter, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideStorage<TEntity> storage;
        private readonly ReadSideCacheSettings settings;

        private bool isCacheEnabled = false;

        protected readonly Dictionary<string, TEntity> cache = new Dictionary<string, TEntity>();

        public MemoryCachedReadSideStorage(IReadSideStorage<TEntity> storage, ReadSideCacheSettings settings)
        {
            this.storage = storage;
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
            => $"{this.storage.GetReadableStatus()}  |  cache {(this.isCacheEnabled ? "enabled" : "disabled")}  |  cached (memory): {this.cache.Count}";

        public Type ViewType => typeof(TEntity);

        public bool IsCacheEnabled => this.isCacheEnabled;

        public void Clear()
        {
            var readSideRepositoryCleaner = this.storage as IReadSideRepositoryCleaner;
            if(readSideRepositoryCleaner!=null)
                readSideRepositoryCleaner.Clear();
        }

        public TEntity GetById(string id)
        {
            return this.isCacheEnabled
                ? this.GetByIdUsingCache(id)
                : this.storage.GetById(id);
        }

        public void Remove(string id)
        {
            if (this.isCacheEnabled)
            {
                this.RemoveUsingCache(id);
            }
            else
            {
                this.storage.Remove(id);
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
                this.storage.Store(view, id);
            }
        }

        public virtual void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            foreach (var item in bulk)
            {
                Store(item.Item1, item.Item2);
            }
        }

        private TEntity GetByIdUsingCache(string id)
        {
            if (cache.ContainsKey(id))
                return cache[id];

            var entity = this.storage.GetById(id);

            cache[id] = entity;

            this.ReduceCacheIfNeeded();

            return entity;
        }

        private void RemoveUsingCache(string id)
        {
            this.cache.Remove(id);
            this.storage.Remove(id);
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
            var bulk = this.cache.Keys.Take(this.settings.StoreOperationBulkSize).ToList();
            this.StoreBulkEntitiesToRepository(bulk);
        }

        protected virtual void StoreBulkEntitiesToRepository(IEnumerable<string> bulk)
        {
            var entitiesToStore = new List<Tuple<TEntity, string>>();

            foreach (var entityId in bulk)
            {
                var entity = cache[entityId];

                if (entity == null)
                {
                    this.storage.Remove(entityId);
                }
                else
                {
                    entitiesToStore.Add(Tuple.Create(entity, entityId));
                }

                cache.Remove(entityId);
            }

            this.storage.BulkStore(entitiesToStore);
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= this.settings.CacheSizeInEntities;
        }
    }
}
