using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using Raven.Abstractions.FileSystem;
using Raven.Client.FileSystem;
using Raven.Json.Linq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    public class RavenFilesStoreRepositoryAccessor<TEntity> : IReadSideKeyValueStorage<TEntity>, IReadSideRepositoryWriter, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly ILogger logger;
        private const int MaxCountOfCachedEntities = 256;
        private const int MaxCountOfEntitiesInOneStoreOperation = 16;
        private readonly ConcurrentDictionary<string, TEntity> cache = new ConcurrentDictionary<string, TEntity>();
        private ConcurrentDictionary<string, bool> packagesInProcess = new ConcurrentDictionary<string, bool>();
        private const int CountOfAttempt = 60;
        private bool isCacheEnabled = false;
        private readonly IJsonUtils jsonUtils;
        private readonly RavenFilesStoreRepositoryAccessorSettings ravenFilesStoreRepositoryAccessorSettings;
        private readonly IWaitService waitService;
        private readonly IFilesStore ravenFilesStore;

        public RavenFilesStoreRepositoryAccessor(ILogger logger, IJsonUtils jsonUtils, IWaitService waitService, RavenFilesStoreRepositoryAccessorSettings ravenFilesStoreRepositoryAccessorSettings)
        {
            this.logger = logger;
            this.jsonUtils = jsonUtils;
            this.waitService = waitService;
            this.ravenFilesStoreRepositoryAccessorSettings = ravenFilesStoreRepositoryAccessorSettings;
            this.ravenFilesStore = this.CreateRavenFilesStore();
        }

        private IFilesStore CreateRavenFilesStore()
        {
            return
                new FilesStore()
                {
                    Url = ravenFilesStoreRepositoryAccessorSettings.Url,
                    DefaultFileSystem = ravenFilesStoreRepositoryAccessorSettings.RavenFileSystemName
                }.Initialize(true);
        }

        public int Count()
        {
            var files = AsyncContext.Run(() =>
            {
                using (var fileSession = ravenFilesStore.OpenAsyncSession())
                {
                    return fileSession.Query().ToListAsync();
                }
            });
            return files.Count;
        }

        TEntity IReadSideStorage<TEntity>.GetById(string id)
        {
            if (!isCacheEnabled)
            {
                if (this.WaitUntilViewCanBeProcessed(id))
                {
                    try
                    {
                        if (ravenFilesStoreRepositoryAccessorSettings.AdditionalEventChecker != null)
                            ravenFilesStoreRepositoryAccessorSettings.AdditionalEventChecker(id);
                    }
                    finally
                    {
                        this.ReleaseSpotForOtherThread(id);
                    }
                }
                return this.GetEntityAvoidingCacheById(id);
            }

            if (!this.cache.ContainsKey(id))
            {
                this.ReduceCacheIfNeeded();

                TEntity entity = this.GetEntityAvoidingCacheById(id);

                this.cache[id] = entity;
            }

            return this.cache[id];
        }

        public void Remove(string id)
        {
            if (this.isCacheEnabled)
            {
                if (this.cache.ContainsKey(id))
                {
                    TEntity deleteResult;
                    cache.TryRemove(id, out deleteResult);
                }
                this.RemoveAvoidingCache(id);

            }
            else
            {
                this.RemoveAvoidingCache(id);
            }

        }

        public void Store(TEntity view, string id)
        {
            if (isCacheEnabled)
                this.StoreUsingCache(view, id);
            else
                this.StoreAvoidingCache(view, id);
        }

        public void EnableCache()
        {
            this.isCacheEnabled = true;
        }

        public void DisableCache()
        {
            this.StoreAllCachedEntitiesToRepository();
            this.isCacheEnabled = false;
        }

        public string GetReadableStatus()
        {
            int cachedEntities = this.cache.Count;

            return string.Format("cache {0};    cached raven file storage items: {1};",
                this.isCacheEnabled ? "enabled" : "disabled",
                cachedEntities);

        }

        public Type ViewType { get { return typeof(TEntity); } }

        public void Clear()
        {
            List<FileHeader> filesToDelete = AsyncContext.Run(() =>
            {
                using (var fileSession = ravenFilesStore.OpenAsyncSession())
                {
                    return fileSession.Query().Where(string.Format("__directoryName: /{0}", ViewType.Name)).ToListAsync();
                }
            });

            foreach (var fileHeader in filesToDelete)
            {
                Remove(fileHeader.Name);
            }
        }

        private void StoreAllCachedEntitiesToRepository()
        {
            foreach (var entityId in cache.Keys)
            {
                var entity = cache[entityId];
                StoreAvoidingCache(entity, entityId);
                TEntity deleteResult;
                cache.TryRemove(entityId, out deleteResult);
            }
            if (!cache.IsEmpty)
                this.StoreAllCachedEntitiesToRepository();
        }

        private void ReleaseSpotForOtherThread(string id)
        {
            bool dummyBool;
            packagesInProcess.TryRemove(id, out dummyBool);
        }

        private bool WaitUntilViewCanBeProcessed(string id)
        {
            int i = 0;
            while (!packagesInProcess.TryAdd(id, true))
            {
                if (i > CountOfAttempt)
                {
                    return false;
                }
                waitService.WaitForSecond();
                i++;
            }
            return true;
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
            var bulk = this.cache.Keys.Take(MaxCountOfEntitiesInOneStoreOperation).ToList();
            foreach (var entityId in bulk)
            {
                var entity = cache[entityId];
                StoreAvoidingCache(entity, entityId);
                TEntity deleteResult;
                cache.TryRemove(entityId, out deleteResult);
            }
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= MaxCountOfCachedEntities;
        }

        private TEntity GetEntityAvoidingCacheById(string entityId)
        {
            using (var stream = AsyncContext.Run(() => ravenFilesStore.AsyncFilesCommands.DownloadAsync(this.CreateFileStoreEntityId(entityId))))
            {
                if (stream == null)
                    return null;

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return jsonUtils.Deserialize<TEntity>(reader.ReadToEnd());
                }
            }
        }

        private void StoreAvoidingCache(TEntity entity, string entityId)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonUtils.Serialize(entity))))
            {
                ravenFilesStore.AsyncFilesCommands.UploadAsync(this.CreateFileStoreEntityId(entityId), memoryStream).WaitAndUnwrapException();
            }
        }

        private void RemoveAvoidingCache(string entityId)
        {
            ravenFilesStore.AsyncFilesCommands.DeleteAsync(this.CreateFileStoreEntityId(entityId)).WaitAndUnwrapException();
        }

        private string CreateFileStoreEntityId(string id)
        {
            return string.Format("{0}/{1}", ViewType.Name, id);
        }

        private void StoreUsingCache(TEntity view, string id)
        {
            this.cache[id] = view;

            this.ReduceCacheIfNeeded();
        }
    }
}
