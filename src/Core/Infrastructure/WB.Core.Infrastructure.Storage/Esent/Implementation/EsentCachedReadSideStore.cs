using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Isam.Esent.Collections.Generic;
using Microsoft.Isam.Esent.Interop;
using Newtonsoft.Json;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Esent.Implementation
{
    internal class EsentCachedKeyValueStorage<TEntity> : EsentCachedReadSideStore<TEntity>,
        IReadSideKeyValueStorage<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        public EsentCachedKeyValueStorage(IReadSideStorage<TEntity> readSideStorage, IFileSystemAccessor fileSystemAccessor)
            : base(readSideStorage, fileSystemAccessor) { }
    }

    internal class EsentCachedReadSideRepositoryWriter<TEntity> : EsentCachedReadSideStore<TEntity>,
        IReadSideRepositoryWriter<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryWriter<TEntity> writer;

        public EsentCachedReadSideRepositoryWriter(IReadSideRepositoryWriter<TEntity> readSideStorage, IFileSystemAccessor fileSystemAccessor)
            : base(readSideStorage, fileSystemAccessor)
        {
            this.writer = readSideStorage;
        }

        //protected override void StoreBulkEntitiesToRepository(IEnumerable<string> bulk)
        //{
        //    var entitiesToStore = new List<Tuple<TEntity, string>>();
        //    foreach (var entityId in bulk)
        //    {
        //        var entity = cache[entityId];
        //        if (entity == null)
        //        {
        //            this.writer.Remove(entityId);
        //        }
        //        else
        //        {
        //            entitiesToStore.Add(Tuple.Create(entity, entityId));
        //        }

        //        cache.Remove(entityId);
        //    }

        //    this.writer.BulkStore(entitiesToStore);
        //}

        public void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            this.writer.BulkStore(bulk);
        }
    }

    internal class EsentCachedReadSideStore<TEntity> : IReadSideStorage<TEntity>, ICacheableRepositoryWriter, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideStorage<TEntity> readSideStorage;

        private bool isCacheEnabled = false;

        private PersistentDictionary<string, string> cache;
        private readonly string collectionFolder;

        public EsentCachedReadSideStore(IReadSideStorage<TEntity> readSideStorage, IFileSystemAccessor fileSystemAccessor)
        {
            this.readSideStorage = readSideStorage;

            string collectionName = typeof(TEntity).Name;
            this.collectionFolder = Path.Combine(@"C:\Projects\AppDatas\HQSV\Temp\Esent", collectionName);

            if (!fileSystemAccessor.IsDirectoryExists(this.collectionFolder))
            {
                fileSystemAccessor.CreateDirectory(this.collectionFolder);
            }

            if (!fileSystemAccessor.IsWritePermissionExists(this.collectionFolder))
            {
                throw new ArgumentException(
                    $"Error initializing ESENT persistent dictionary because there are problems with write access to folder {this.collectionFolder}");
            }

            this.cache = new PersistentDictionary<string, string>(collectionFolder);
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
            var readSideRepositoryCleaner = this.readSideStorage as IReadSideRepositoryCleaner;
            if(readSideRepositoryCleaner!=null)
                readSideRepositoryCleaner.Clear();
        }

        public TEntity GetById(string id)
        {
            return this.isCacheEnabled
                ? this.GetByIdUsingCache(id)
                : this.readSideStorage.GetById(id);
        }

        public void Remove(string id)
        {
            if (this.isCacheEnabled)
            {
                this.RemoveUsingCache(id);
            }
            else
            {
                this.readSideStorage.Remove(id);
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
                this.readSideStorage.Store(view, id);
            }
        }

        private TEntity GetByIdUsingCache(string id)
        {
            TEntity entityFromCache = this.GetFromCache(id);

            if (entityFromCache != null)
                return entityFromCache;

            var entity = this.readSideStorage.GetById(id);

            this.cache[id] = Serialize(entity);

            return entity;
        }

        private TEntity GetFromCache(string id)
        {
            string value;
            if (this.cache.TryGetValue(id, out value))
                return value != null ? Deserialize(value) : null;
            else
                return null;
        }

        private static TEntity Deserialize(string value)
        {
            return JsonConvert.DeserializeObject<TEntity>(value, JsonSerializerSettings);
        }

        private void RemoveUsingCache(string id)
        {
            this.cache.Remove(id);
            this.readSideStorage.Remove(id);
        }

        private void StoreUsingCache(TEntity entity, string id)
        {
            this.cache[id] = Serialize(entity);
        }

        private static string Serialize(TEntity entity)
        {
            return JsonConvert.SerializeObject(entity, Formatting.None, JsonSerializerSettings);
        }

        protected virtual void StoreBulkEntitiesToRepository(IEnumerable<string> bulk)
        {
            foreach (var entityId in bulk)
            {
                TEntity entity = this.GetFromCache(entityId);
                if (entity == null)
                {
                    this.readSideStorage.Remove(entityId);
                }
                else
                {
                    this.readSideStorage.Store(entity, entityId);
                }

                this.cache.Remove(entityId);
            }
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
                if (this.cache != null)
                {
                    this.cache.Dispose();
                    this.cache = null;
                }

                // free native resources if there are any.
            }
        }

        private static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
            }
        }
    }
}