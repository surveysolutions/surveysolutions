using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Isam.Esent.Collections.Generic;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Infrastructure.Native.Storage.Esent.Implementation
{
    internal class EsentCachedReadSideStorage<TEntity> : IReadSideKeyValueStorage<TEntity>, IReadSideRepositoryWriter<TEntity>, ICacheableRepositoryWriter, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideStorage<TEntity> storage;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ReadSideCacheSettings cacheSettings;

        private bool isCacheUsed = false;
        private int entitiesSentToStorage = 0;

        private readonly Dictionary<string, TEntity> memoryCache = new Dictionary<string, TEntity>();
        private PersistentDictionary<string, string> esentCache;
        private readonly HashSet<string> savedToEsentIds = new HashSet<string>();

        public EsentCachedReadSideStorage(IReadSideStorage<TEntity> storage, IFileSystemAccessor fileSystemAccessor, ReadSideCacheSettings cacheSettings)
        {
            this.storage = storage;
            this.fileSystemAccessor = fileSystemAccessor;
            this.cacheSettings = cacheSettings;
        }

        public Type ViewType => typeof(TEntity);

        private string EsentCacheFolder => Path.Combine(this.cacheSettings.EsentCacheFolder, this.ViewType.Name);

        public string GetReadableStatus()
            => $"{this.storage.GetReadableStatus()}  |  cache {(this.isCacheUsed ? "enabled" : "disabled")}  |  memory ⇄ ESENT → storage : {this.memoryCache.Count:N0} ⇄ {this.esentCache?.Count.ToString("N0") ?? "N/A"} → {this.entitiesSentToStorage:N0}";

        public bool IsCacheEnabled => this.isCacheUsed;

        public void Clear() => (this.storage as IReadSideRepositoryCleaner)?.Clear();

        public void EnableCache()
        {
            this.InitializeEsentCache();

            this.isCacheUsed = true;

            this.entitiesSentToStorage = 0;

            // TODO: fill esent cache with data from actual storage
        }

        public void DisableCache()
        {
            this.MoveEntitiesFromMemoryToEsent(leaveEntities: 0);

            this.MoveEntitiesFromEsentToStorage();

            this.isCacheUsed = false;
        }

        private void InitializeEsentCache()
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(this.EsentCacheFolder))
            {
                this.fileSystemAccessor.CreateDirectory(this.EsentCacheFolder);
            }

            if (!this.fileSystemAccessor.IsWritePermissionExists(this.EsentCacheFolder))
            {
                throw new ArgumentException(
                    $"Error initializing ESENT persistent dictionary because there are problems with write access to folder {this.EsentCacheFolder}");
            }

            PersistentDictionaryFile.DeleteFiles(this.EsentCacheFolder);

            this.esentCache = new PersistentDictionary<string, string>(this.EsentCacheFolder);
        }

        public TEntity GetById(string id)
        {
            if (this.isCacheUsed)
            {
                return this.GetFromCache(id);
            }
            else
            {
                return this.storage.GetById(id);
            }
        }

        public void Remove(string id)
        {
            if (this.isCacheUsed)
            {
                this.RemoveFromCache(id);
            }
            else
            {
                this.storage.Remove(id);
            }
        }

        public void RemoveIfStartsWith(string beginingOfId)
        {
            if (this.isCacheUsed)
            {
                var allKeyToRemoveFromCache = this.memoryCache.Keys.Where(k => k.StartsWith(beginingOfId)).ToArray();
                foreach (var keyToRemove in allKeyToRemoveFromCache)
                {
                    this.memoryCache[keyToRemove]=null;
                }

                var allKeyToRemoveFromEsent = this.savedToEsentIds.Where(k => k.StartsWith(beginingOfId)).Except(allKeyToRemoveFromCache).ToArray();
                foreach (var keyToRemove in allKeyToRemoveFromEsent)
                {
                    this.memoryCache[keyToRemove] = null;
                }

            }
            else
            {
                this.storage.RemoveIfStartsWith(beginingOfId);
            }
        }

        public void Store(TEntity view, string id)
        {
            if (this.isCacheUsed)
            {
                this.StoreToCache(view, id);
            }
            else
            {
                this.storage.Store(view, id);
            }
        }

        public void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            if (this.isCacheUsed)
            {
                foreach (var tuple in bulk)
                {
                    this.StoreToCache(tuple.Item1, tuple.Item2);
                }
            }
            else
            {
                this.storage.BulkStore(bulk);
            }
        }

        private TEntity GetFromCache(string id)
        {
            try
            {
                if (this.memoryCache.ContainsKey(id))
                    return this.memoryCache[id];

                string value;
                if (!this.esentCache.TryGetValue(id, out value))
                    return null;

                var entity = Deserialize(value);

                this.memoryCache[id] = entity;

                return entity;
            }
            finally
            {
                this.ReduceMemoryCacheIfNeeded();
            }
        }

        private void RemoveFromCache(string id)
        {
            this.memoryCache[id] = null;
        }

        private void StoreToCache(TEntity entity, string id)
        {
            try
            {
                this.memoryCache[id] = entity;
            }
            finally
            {
                this.ReduceMemoryCacheIfNeeded();
            }
        }

        private void ReduceMemoryCacheIfNeeded()
        {
            if (this.memoryCache.Count >= this.cacheSettings.CacheSizeInEntities)
            {
                this.MoveEntitiesFromMemoryToEsent(leaveEntities: this.memoryCache.Count / 2);
            }
        }

        private void MoveEntitiesFromMemoryToEsent(int leaveEntities)
        {
            var entityIdsToRemoveFromCache = this.memoryCache.Keys.Skip(leaveEntities).ToList();

            foreach (string entityId in entityIdsToRemoveFromCache)
            {
                TEntity entity = this.memoryCache[entityId];

                if (entity == null)
                {
                    this.esentCache.Remove(entityId);
                    savedToEsentIds.Remove(entityId);
                }
                else
                {
                    this.esentCache[entityId] = Serialize(entity);
                    savedToEsentIds.Add(entityId);
                }

                this.memoryCache.Remove(entityId);
            }
        }

        private void MoveEntitiesFromEsentToStorage()
        {
            var bulks = this
                .esentCache
                .Select(pair => Tuple.Create(Deserialize(pair.Value), pair.Key))
                .Batch(this.cacheSettings.StoreOperationBulkSize)
                .Select(bulk => bulk.ToList());

            foreach (var bulk in bulks)
            {
                this.storage.BulkStore(bulk);
                this.entitiesSentToStorage += bulk.Count;
            }

            this.esentCache.Dispose();
            PersistentDictionaryFile.DeleteFiles(this.EsentCacheFolder);
            this.esentCache = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (this.esentCache != null)
                {
                    this.esentCache.Dispose();
                    this.esentCache = null;
                }

                // free native resources if there are any.
            }
        }

        private static TEntity Deserialize(string value) => JsonConvert.DeserializeObject<TEntity>(value, JsonSerializerSettings);

        private static string Serialize(TEntity entity) => JsonConvert.SerializeObject(entity, Formatting.None, JsonSerializerSettings);

        private static JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };
    }
}