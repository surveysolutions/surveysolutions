using System;
using System.Collections.Concurrent;
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using Raven.Imports.Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    #warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryWriter<TEntity> : RavenReadSideRepositoryAccessor<TEntity>, IReadSideRepositoryWriter<TEntity>, IReadSideRepositoryWriter, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        private const int MaxCountOfCachedEntities = 256;
        private const int MaxCountOfEntitiesInOneStoreOperation = 16;
        private const string ReadSideCacheFolderName = "ReadSideCache";

        private bool isCacheEnabled = false;
        private string cacheFolderUniqueId;

        private readonly ConcurrentDictionary<string, TEntity> cache = new ConcurrentDictionary<string, TEntity>();
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly RavenReadSideRepositoryWriterSettings settings;
        private readonly string cachedViewsFolder;
        private readonly ILogger logger;

        private JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ContractResolver = this.RavenStore.Conventions.JsonContractResolver,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
            }
        }

        public RavenReadSideRepositoryWriter(IDocumentStore ravenStore, IFileSystemAccessor fileSystemAccessor, ILogger logger, RavenReadSideRepositoryWriterSettings settings)
            : base(ravenStore)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.settings = settings;

            var readSideCacheFolderPath = GetFolderPathAndCreateIfAbsent(settings.BasePath, ReadSideCacheFolderName);

            this.cachedViewsFolder = GetFolderPathAndCreateIfAbsent(readSideCacheFolderPath, ViewName);
        }

        private string GetFolderPathAndCreateIfAbsent(string path, string folderName)
        {
            var folderPath = fileSystemAccessor.CombinePath(path, folderName);
            if (!fileSystemAccessor.IsDirectoryExists(folderPath))
                fileSystemAccessor.CreateDirectory(folderPath);
            return folderPath;
        }

        public TEntity GetById(string id)
        {
            return this.isCacheEnabled
                ? this.GetByIdUsingCache(id)
                : this.GetByIdAvoidingCache(id);
        }

        public void Remove(string id)
        {
            if (this.isCacheEnabled)
            {
                this.RemoveUsingCache(id);
            }
            else
            {
                this.RemoveAvoidingCache(id);
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
                this.StoreAvoidingCache(view, id);
            }
        }

        public void EnableCache()
        {
            this.isCacheEnabled = true;

            this.cacheFolderUniqueId = Guid.NewGuid().FormatGuid();

            var cacheDirectory = fileSystemAccessor.CombinePath(cachedViewsFolder, this.cacheFolderUniqueId);

            if (fileSystemAccessor.IsDirectoryExists(cacheDirectory))
                fileSystemAccessor.DeleteDirectory(cacheDirectory);

            fileSystemAccessor.CreateDirectory(cacheDirectory);
        }

        public void DisableCache()
        {
            this.StoreAllCachedEntitiesToRepository();

            this.isCacheEnabled = false;

            this.cacheFolderUniqueId = string.Empty;
        }

        public string GetReadableStatus()
        {
            int cachedEntities = this.cache.Count + fileSystemAccessor.GetFilesInDirectory(fileSystemAccessor.CombinePath(cachedViewsFolder, cacheFolderUniqueId)).Length;

            int cachedEntitiesWhichNeedToBeStoredToRepository = this.cache.Count;

            return string.Format("cache {0,8};    cached: {1,3};    not stored: {2,3}",
                this.isCacheEnabled ? "enabled" : "disabled",
                cachedEntities,
                cachedEntitiesWhichNeedToBeStoredToRepository);
        }

        public Type ViewType
        {
            get { return typeof (TEntity); }
        }

        private TEntity GetByIdUsingCache(string id)
        {
            var result = cache.GetOrAdd(id, (key) =>
            {
                TEntity entity = null;

                var filePath = this.GetPathToEntity(id);

                if (fileSystemAccessor.IsFileExists(filePath))
                {
                    entity = this.Deserrialize(fileSystemAccessor.ReadAllText(filePath));
                    fileSystemAccessor.DeleteFile(filePath);
                }

                if (entity==null)
                    entity = GetByIdAvoidingCache(key);

                return entity;
            });

            this.ReduceCacheIfNeeded();

            return result;
        }

        private void RemoveUsingCache(string id)
        {
            TEntity entity;
            this.cache.TryRemove(id, out entity);

            var filePath = this.GetPathToEntity(id);

            if (fileSystemAccessor.IsFileExists(filePath))
            {
                fileSystemAccessor.DeleteFile(filePath);
            }
        }

        private void StoreUsingCache(TEntity entity, string id)
        {
            this.cache.AddOrUpdate(id, (key) => entity, (key, value) => entity);

            var filePath = this.GetPathToEntity(id);

            if (fileSystemAccessor.IsFileExists(filePath))
            {
                fileSystemAccessor.DeleteFile(filePath);
            }

            this.ReduceCacheIfNeeded();
        }

        private TEntity GetByIdAvoidingCache(string id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                var result = session.Load<TEntity>(id: ravenId);

                return result;
            }
        }

        private void RemoveAvoidingCache(string id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                var view = session.Load<TEntity>(id: ravenId);
                
                if(view==null)
                    return;

                session.Delete(view);
                session.SaveChanges();
            }
        }

        private void StoreAvoidingCache(TEntity entity, string id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                session.Store(entity: entity, id: ravenId);
                session.SaveChanges();
            }
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

            foreach (var cachedEntityId in bulk)
            {
                TEntity entity;

                if (cache.TryRemove(cachedEntityId, out entity))
                {
                    this.fileSystemAccessor.WriteAllText(this.GetPathToEntity(cachedEntityId),
                        this.GetItemAsContent(entity));
                }
            }
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= MaxCountOfCachedEntities;
        }

        private void StoreAllCachedEntitiesToRepository()
        {
            try
            {
                using (var session = this.RavenStore.BulkInsert(options: new BulkInsertOptions() { OverwriteExisting = true }))
                {
                    foreach (var entity in cache)
                    {
                        StoreCachedEntityToRepository(session, entity.Key, entity.Value);
                    }
                }

                cache.Clear();
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);

                foreach (var entity in cache)
                {
                    this.fileSystemAccessor.WriteAllText(this.GetPathToEntity(entity.Key),
                     this.GetItemAsContent(entity.Value));
                }
                throw;
            }

            var fileNamesWithCachedEntities =
                fileSystemAccessor.GetFilesInDirectory(fileSystemAccessor.CombinePath(cachedViewsFolder, cacheFolderUniqueId));

            using (var session = this.RavenStore.BulkInsert(options: new BulkInsertOptions() { OverwriteExisting = true, BatchSize = settings.BulkInsertBatchSize }))
            {
                foreach (var fileNamesWithCachedEntity in fileNamesWithCachedEntities)
                {
                    StoreCachedEntityToRepository(session, fileSystemAccessor.GetFileName(fileNamesWithCachedEntity),
                        this.Deserrialize(fileSystemAccessor.ReadAllText(fileNamesWithCachedEntity)));
                }
            }
            this.fileSystemAccessor.DeleteDirectory(fileSystemAccessor.CombinePath(cachedViewsFolder, this.cacheFolderUniqueId));
        }

        private void StoreCachedEntityToRepository(BulkInsertOperation bulkOperation, string id, TEntity entity)
        {
            if(entity==null)
                return;
            string ravenId = ToRavenId(id);
            bulkOperation.Store(entity: entity, id: ravenId);
        }

        private string GetPathToEntity(string entityName)
        {
            return fileSystemAccessor.CombinePath(fileSystemAccessor.CombinePath(cachedViewsFolder, cacheFolderUniqueId), entityName);
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
                throw new InvalidOperationException(payload, e);
            }
        }

        public void Clear()
        {
            const string DefaultIndexName = "Raven/DocumentsByEntityName";
            if (this.RavenStore.DatabaseCommands.GetIndex(DefaultIndexName) != null)
                this.RavenStore.DatabaseCommands.DeleteByIndex(DefaultIndexName, new IndexQuery()
                {
                    Query = string.Format("Tag: *{0}*", global::Raven.Client.Util.Inflector.Pluralize(ViewName))
                }, new BulkOperationOptions {AllowStale = false}).WaitForCompletion();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        protected override TResult QueryImpl<TResult>(Func<IRavenQueryable<TEntity>, TResult> query)
        {
            throw new NotImplementedException();
        }
    }
}