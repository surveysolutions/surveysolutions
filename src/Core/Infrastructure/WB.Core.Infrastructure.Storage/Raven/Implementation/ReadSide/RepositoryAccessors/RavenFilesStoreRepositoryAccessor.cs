using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using Raven.Abstractions.FileSystem;
using Raven.Client.FileSystem;
using Raven.Imports.Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    public class RavenFilesStoreRepositoryAccessor<TEntity> : IReadSideKeyValueStorage<TEntity>, IReadSideRepositoryWriter, IReadSideRepositoryCleaner, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly ILogger logger;
        private const int MaxCountOfCachedEntities = 256;
        private const int MaxCountOfEntitiesInOneStoreOperation = 30;
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly ConcurrentDictionary<string, TEntity> cache = new ConcurrentDictionary<string, TEntity>();
        private readonly IAdditionalDataService<TEntity> additionalDataService;
        private bool isCacheEnabled = false;
        bool disposed;
        private readonly IFilesStore ravenFileStore;

        public RavenFilesStoreRepositoryAccessor(ILogger logger, IFilesStore ravenFileStore)
        {
            this.logger = logger;
            this.ravenFileStore = ravenFileStore;
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
                using (var fileSession = ravenFileStore.OpenAsyncSession())
                {
                    return fileSession.Query().Where(string.Format("__directoryName: /{0}", ViewType.Name)).ToListAsync();
                }
            });

            foreach (var fileHeader in filesToDelete)
            {
                Remove(fileHeader.Name);
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RavenFilesStoreRepositoryAccessor()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                this.ravenFileStore.Dispose();
            }

            this.disposed = true;
        }

        private void StoreAllCachedEntitiesToRepository()
        {
            while (!cache.IsEmpty)
                ReduceCache();
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
            StoreChunk(this.cache.Keys.Take(MaxCountOfEntitiesInOneStoreOperation).ToList());
        }

        private void StoreChunk(List<string> bulk)
        {
            var streamsToClose = new Dictionary<string, MemoryStream>();
            using (var session = this.ravenFileStore.OpenAsyncSession())
            {
                foreach (var entityId in bulk)
                {
                    var entityToStore = cache[entityId];
                    if(entityToStore==null)
                        continue;
                    
                    var memoryStream =
                        new MemoryStream(Encoding.UTF8.GetBytes(this.GetItemAsContent(entityToStore)));
                    session.RegisterUpload(this.CreateFileStoreEntityId(entityId), memoryStream);

                    streamsToClose.Add(entityId, memoryStream);
                }
                session.SaveChangesAsync().WaitAndUnwrapException(); ;
            }

            foreach (var entityId in bulk)
            {
                streamsToClose[entityId].Dispose();
                streamsToClose.Remove(entityId);

                TEntity deleteResult;
                this.cache.TryRemove(entityId, out deleteResult);
            }
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= MaxCountOfCachedEntities;
        }

        private TEntity GetEntityAvoidingCacheById(string entityId)
        {
            try
            {
                using (
                    var stream =
                        AsyncContext.Run(() => ravenFileStore.AsyncFilesCommands.DownloadAsync(this.CreateFileStoreEntityId(entityId))))
                {
                    using (var reader = new StreamReader(stream, encoding))
                    {
                        return Deserrialize(reader.ReadToEnd());
                    }
                }
            }
            catch (FileNotFoundException)
            {
                //it's ok to have FileNotFoundException, view could be absent
            }
            return null;
        }

        private void StoreAvoidingCache(TEntity entity, string entityId)
        {
            using (var memoryStream = new MemoryStream(encoding.GetBytes(GetItemAsContent(entity))))
            {
                ravenFileStore.AsyncFilesCommands.UploadAsync(this.CreateFileStoreEntityId(entityId), memoryStream)
                    .WaitAndUnwrapException();
            }
        }

        private void RemoveAvoidingCache(string entityId)
        {
            try
            {
                ravenFileStore.AsyncFilesCommands.DeleteAsync(this.CreateFileStoreEntityId(entityId)).WaitAndUnwrapException();
            }
            catch (FileNotFoundException e)
            {
                //it's ok to have FileNotFoundException during rebuild readside
                logger.Info(e.Message, e);
            }
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

        private JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ContractResolver = new PropertiesOnlyContractResolver(),
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
            }
        }

        private string GetItemAsContent(TEntity item)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, JsonSerializerSettings);
        }

        private TEntity Deserrialize(string payload)
        {
            try
            {
                return JsonConvert.DeserializeObject<TEntity>(payload, JsonSerializerSettings);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new InvalidOperationException(payload, e);
            }
        }
    }
}
