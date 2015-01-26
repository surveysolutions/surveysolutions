using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using Raven.Abstractions.Extensions;
using Raven.Abstractions.FileSystem;
using Raven.Client.FileSystem;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    internal class RavenFilesStoreRepositoryAccessor<TEntity> : IReadSideKeyValueStorage<TEntity>, IReadSideRepositoryWriter, IReadSideRepositoryCleaner, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly ILogger logger;
        private const int MaxCountOfCachedEntities = 1024;
        private const int MaxCountOfEntitiesInOneStoreOperation = 30;
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly Dictionary<string, TEntity> cache = new Dictionary<string, TEntity>();
        private readonly IAdditionalDataService<TEntity> additionalDataService;
        private bool isCacheEnabled = false;
        private readonly IFilesStore ravenFilesStore;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public RavenFilesStoreRepositoryAccessor(ILogger logger, IFilesStore ravenFileStore)
        {
            this.logger = logger;
            this.ravenFilesStore = this.CreateRavenFilesStore();

            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        private IFilesStore CreateRavenFilesStore()
        {
            return
                new FilesStore()
                {
                }.Initialize(true);
        }

        public RavenFilesStoreRepositoryAccessor(ILogger logger, IFilesStore ravenFileStore,
            IAdditionalDataService<TEntity> additionalDataService) : this(logger, ravenFileStore)
        {
            this.additionalDataService = additionalDataService;
        }

        public TEntity GetById(string id)
        {
            if (!isCacheEnabled)
            {
                if (additionalDataService != null)
                    additionalDataService.CheckAdditionalRepository(id);
                return AsyncContext.Run(() => this.GetEntityAvoidingCacheById(id));
            }

            if (!this.cache.ContainsKey(id))
            {
                this.ReduceCacheIfNeeded();

                TEntity entity = AsyncContext.Run(() => this.GetEntityAvoidingCacheById(id));

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
                    cache.Remove(id);
                }
                this.RemoveAvoidingCache(id).WaitAndUnwrapException();

            }
            else
            {
                this.RemoveAvoidingCache(id).WaitAndUnwrapException();
            }

        }

        public void Store(TEntity view, string id)
        {
            if (isCacheEnabled)
                this.StoreUsingCache(view, id);
            else
                this.StoreAvoidingCache(view, id).WaitAndUnwrapException();
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
                ravenFilesStore.AsyncFilesCommands.DeleteAsync(this.CreateFileStoreEntityId(fileHeader.Name)).WaitAndUnwrapException();
            }
        }

        private void StoreAllCachedEntitiesToRepository()
        {
            while (cache.Any())
                StoreBulkOfViews(this.cache.Keys.ToList());
        }

        private void ReduceCacheIfNeeded()
        {
            if (this.IsCacheLimitReached())
            {
                this.ReduceCache();}
        }

        private void ReduceCache()
        {
            StoreBulkOfViews(this.cache.Keys.Take(MaxCountOfEntitiesInOneStoreOperation).ToList());
        }

        private void StoreBulkOfViews(List<string> bulk)
        {
            foreach (var entityId in bulk)
            {
                StoreEntityAsync(entityId).WaitAndUnwrapException();
            }
        }

        private async System.Threading.Tasks.Task StoreEntityAsync(string entityId)
        {
            var entityToStore = cache[entityId];
            if (entityToStore == null)
                return;

            await StoreAvoidingCache(entityToStore, entityId).ConfigureAwait(false);
            cache.Remove(entityId);
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= MaxCountOfCachedEntities;
        }

        private async Task<TEntity> GetEntityAvoidingCacheById(string entityId)
        {
            var fileEntityId = this.CreateFileStoreEntityId(entityId);
            var headers = await ravenFilesStore.AsyncFilesCommands.GetAsync(new[] { fileEntityId }).ConfigureAwait(false);
            if (!headers.Any())
                return null;

            using (
                var stream =
                    await ravenFilesStore.AsyncFilesCommands.DownloadAsync(fileEntityId).ConfigureAwait(false))
            {
                using (var reader = new StreamReader(stream, encoding))
                {
                    return Deserrialize(reader.ReadToEnd());
                }
            }
        }

        private async System.Threading.Tasks.Task StoreAvoidingCache(TEntity entity, string entityId)
        {
            using (var memoryStream = new MemoryStream(encoding.GetBytes(GetItemAsContent(entity))))
            {
                await ravenFilesStore.AsyncFilesCommands.UploadAsync(this.CreateFileStoreEntityId(entityId), memoryStream).ConfigureAwait(false);
            }
        }

        private async System.Threading.Tasks.Task RemoveAvoidingCache(string entityId)
        {
            var fileEntityId = this.CreateFileStoreEntityId(entityId);
            var headers = await ravenFilesStore.AsyncFilesCommands.GetAsync(new[] { fileEntityId }).ConfigureAwait(false);
            if (!headers.Any())
                return;
            await ravenFilesStore.AsyncFilesCommands.DeleteAsync(fileEntityId).ConfigureAwait(false);
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

        private string GetItemAsContent(TEntity item)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, jsonSerializerSettings);
        }

        private TEntity Deserrialize(string payload)
        {
            try
            {
                return JsonConvert.DeserializeObject<TEntity>(payload, jsonSerializerSettings);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new InvalidOperationException(payload, e);
            }
        }

        public void Dispose()
        {
            if (ravenFilesStore != null)
            {
                ravenFilesStore.Dispose();
            }
        }
    }
}
