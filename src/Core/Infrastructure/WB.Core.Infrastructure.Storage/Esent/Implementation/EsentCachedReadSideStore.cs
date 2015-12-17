using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Isam.Esent.Collections.Generic;
using Microsoft.Isam.Esent.Interop;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Esent.Implementation
{
    internal class EsentCachedKeyValueStorage<TEntity> : EsentCachedReadSideStore<TEntity>,
        IReadSideKeyValueStorage<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        public EsentCachedKeyValueStorage(IReadSideStorage<TEntity> storage, IFileSystemAccessor fileSystemAccessor, ReadSideStoreMemoryCacheSettings memoryCacheSettings)
            : base(storage, fileSystemAccessor, memoryCacheSettings) {}
    }

    internal class EsentCachedReadSideRepositoryWriter<TEntity> : EsentCachedReadSideStore<TEntity>,
        IReadSideRepositoryWriter<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryWriter<TEntity> writer;

        public EsentCachedReadSideRepositoryWriter(IReadSideRepositoryWriter<TEntity> storage, IFileSystemAccessor fileSystemAccessor, ReadSideStoreMemoryCacheSettings memoryCacheSettings)
            : base(storage, fileSystemAccessor, memoryCacheSettings)
        {
            this.writer = storage;
        }
    }

    internal class EsentCachedReadSideStore<TEntity> : IReadSideStorage<TEntity>, ICacheableRepositoryWriter, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideStorage<TEntity> storage;
        private readonly ReadSideStoreMemoryCacheSettings memoryCacheSettings;

        private bool isCacheUsed = false;

        private readonly Dictionary<string, TEntity> memoryCache = new Dictionary<string, TEntity>();
        private PersistentDictionary<string, string> esentCache;
        private readonly string esentCacheFolder;

        public EsentCachedReadSideStore(IReadSideStorage<TEntity> storage, IFileSystemAccessor fileSystemAccessor, ReadSideStoreMemoryCacheSettings memoryCacheSettings)
        {
            this.storage = storage;
            this.memoryCacheSettings = memoryCacheSettings;

            this.esentCacheFolder = Path.Combine(@"C:\Projects\AppDatas\HQSV\Temp\Esent", typeof(TEntity).Name);

            if (!fileSystemAccessor.IsDirectoryExists(this.esentCacheFolder))
            {
                fileSystemAccessor.CreateDirectory(this.esentCacheFolder);
            }

            if (!fileSystemAccessor.IsWritePermissionExists(this.esentCacheFolder))
            {
                throw new ArgumentException(
                    $"Error initializing ESENT persistent dictionary because there are problems with write access to folder {this.esentCacheFolder}");
            }

            PersistentDictionaryFile.DeleteFiles(this.esentCacheFolder);
            this.esentCache = new PersistentDictionary<string, string>(this.esentCacheFolder);
        }

        public string GetReadableStatus()
            => $"{this.storage.GetReadableStatus()}  |  cache {(this.isCacheUsed ? "memory + ESENT" : "disabled")}  |  cached {this.memoryCache.Count}, {this.esentCache.Count}";

        public Type ViewType => typeof(TEntity);

        public bool IsCacheEnabled => this.isCacheUsed;

        public void Clear() => (this.storage as IReadSideRepositoryCleaner)?.Clear();

        public void EnableCache()
        {
            this.isCacheUsed = true;

            // TODO: fill esent cache with data from actual storage
        }

        public void DisableCache()
        {
            this.MoveEntitiesFromMemoryToEsent(leaveEntities: 0);

            this.MoveEntitiesFromEsentToStorage();

            this.isCacheUsed = false;
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
            if (this.memoryCache.Count >= this.memoryCacheSettings.MaxCountOfCachedEntities)
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
                }
                else
                {
                    this.esentCache[entityId] = Serialize(entity);
                }

                this.memoryCache.Remove(entityId);
            }
        }

        private void MoveEntitiesFromEsentToStorage()
        {
            var bulks = this
                .esentCache
                .Select(pair => Tuple.Create(Deserialize(pair.Value), pair.Key))
                .Batch(this.memoryCacheSettings.MaxCountOfEntitiesInOneStoreOperation)
                .Select(bulk => bulk.ToList());

            foreach (var bulk in bulks)
            {
                this.storage.BulkStore(bulk);
            }

            this.esentCache.Dispose();
            PersistentDictionaryFile.DeleteFiles(this.esentCacheFolder);
            this.esentCache = new PersistentDictionary<string, string>(this.esentCacheFolder);
        }

        public void Dispose()
        {
            Dispose(true);
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