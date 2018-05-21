using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Infrastructure.Native.Storage.Memory.Implementation
{
    internal class MemoryCachedReadSideStorage<TEntity, TKey> 
        :   IReadSideRepositoryWriter<TEntity, TKey>, 
            IReadSideKeyValueStorage<TEntity, TKey>, 
            ICacheableRepositoryWriter
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideStorage<TEntity, TKey> storage;
        private readonly ReadSideCacheSettings settings;

        private bool isCacheEnabled = false;

        protected readonly Dictionary<TKey, TEntity> cache = new Dictionary<TKey, TEntity>();

        public MemoryCachedReadSideStorage(IReadSideStorage<TEntity, TKey> storage, ReadSideCacheSettings settings)
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
            while (this.cache.Any())
            {
                this.StoreBulkEntitiesToRepository(this.cache.Keys.ToList());
            }

            this.isCacheEnabled = false;
        }

        public string GetReadableStatus()
            => $"{this.storage.GetReadableStatus()}  |  cache {(this.isCacheEnabled ? "enabled" : "disabled")}  |  cached (memory): {this.cache.Count}";

        public Type ViewType => typeof(TEntity);

        public bool IsCacheEnabled => this.isCacheEnabled;

        public TEntity GetById(TKey id)
        {
            return this.isCacheEnabled
                ? this.GetByIdUsingCache(id)
                : this.storage.GetById(id);
        }

        public void Remove(TKey id)
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

        public void Store(TEntity view, TKey id)
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

        public virtual void BulkStore(List<Tuple<TEntity, TKey>> bulk)
        {
            foreach (var item in bulk)
            {
                this.Store(item.Item1, item.Item2);
            }
        }

        public void Flush()
        {
            if (!this.isCacheEnabled) this.storage.Flush();
        }

        private TEntity GetByIdUsingCache(TKey id)
        {
            if (this.cache.ContainsKey(id))
                return this.cache[id];

            var entity = this.storage.GetById(id);

            this.cache[id] = entity;

            this.ReduceCacheIfNeeded();

            return entity;
        }

        private void RemoveUsingCache(TKey id)
        {
            this.cache.Remove(id);
            this.storage.Remove(id);
        }

        private void StoreUsingCache(TEntity entity, TKey id)
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

        protected virtual void StoreBulkEntitiesToRepository(IEnumerable<TKey> bulk)
        {
            var entitiesToStore = new List<Tuple<TEntity, TKey>>();

            foreach (var entityId in bulk)
            {
                var entity = this.cache[entityId];

                if (entity == null)
                {
                    this.storage.Remove(entityId);
                }
                else
                {
                    entitiesToStore.Add(Tuple.Create(entity, entityId));
                }

                this.cache.Remove(entityId);
            }

            this.storage.BulkStore(entitiesToStore);
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= this.settings.CacheSizeInEntities;
        }
    }

    internal class MemoryCachedReadSideStorage<TEntity> : MemoryCachedReadSideStorage<TEntity, string>, 
        IReadSideRepositoryWriter<TEntity>,
        IReadSideKeyValueStorage<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        public MemoryCachedReadSideStorage(IReadSideStorage<TEntity, string> storage, ReadSideCacheSettings settings) 
            : base(storage, settings)
        {
        }
    }
}
