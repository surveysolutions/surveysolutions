using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using Raven.Imports.Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    #warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryWriter<TEntity> : RavenReadSideRepositoryAccessor<TEntity>, IQueryableReadSideRepositoryWriter<TEntity>, IReadSideRepositoryWriter, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        private const int MaxCountOfCachedEntities = 256;
        private const int MaxCountOfEntitiesInOneStoreOperation = 16;
        private const string ReadSideCacheFolderName = "ReadSideCache";

        private bool isCacheEnabled = false;
        private string cacheFolderUniqueId;

        private readonly ConcurrentDictionary<string, TEntity> cache = new ConcurrentDictionary<string, TEntity>();
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string basePath;

        public RavenReadSideRepositoryWriter(DocumentStore ravenStore, IFileSystemAccessor fileSystemAccessor, string basePath)
            : base(ravenStore)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            var readSideCacheFolderPath = GetFolderPathAndCreateIfAbsent(basePath, ReadSideCacheFolderName);

            this.basePath = GetFolderPathAndCreateIfAbsent(readSideCacheFolderPath, ViewName);
        }

        private string GetFolderPathAndCreateIfAbsent(string path, string folderName)
        {
            var folderPath = fileSystemAccessor.CombinePath(path, folderName);
            if (!fileSystemAccessor.IsDirectoryExists(folderPath))
                fileSystemAccessor.CreateDirectory(folderPath);
            return folderPath;
        }

        protected override TResult QueryImpl<TResult>(Func<IRavenQueryable<TEntity>, TResult> query)
        {
            if (this.isCacheEnabled)
                this.StoreAllCachedEntitiesToRepository();

            using (IDocumentSession session = this.OpenSession())
            {
                return query.Invoke(
                    session
                        .Query<TEntity>()
                        .Customize(customization => customization.WaitForNonStaleResultsAsOfLastWrite()));
            }
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

            var cacheDirectory = fileSystemAccessor.CombinePath(basePath, this.cacheFolderUniqueId);

            if(fileSystemAccessor.IsDirectoryExists(cacheDirectory))
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
            int cachedEntities = this.cache.Count+fileSystemAccessor.GetFilesInDirectory(fileSystemAccessor.CombinePath(basePath,cacheFolderUniqueId)).Length;

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
            using (var session = this.ravenStore.BulkInsert())
            {
                var restOfCaches = cache.Keys.ToList();

                while (restOfCaches.Count > 0)
                {
                    foreach (var cachedEntityId in restOfCaches)
                    {
                        TEntity entity;
                        if (cache.TryRemove(cachedEntityId, out entity))
                        {
                            StoreCachedEntityToRepository(session, cachedEntityId, entity);
                        }
                    }
                    restOfCaches = cache.Keys.ToList();
                } 

                var fileNamesWithCachedEntities =
                    fileSystemAccessor.GetFilesInDirectory(fileSystemAccessor.CombinePath(basePath, cacheFolderUniqueId));

                while (fileNamesWithCachedEntities.Length > 0)
                {
                    foreach (var fileNamesWithCachedEntity in fileNamesWithCachedEntities)
                    {
                        StoreCachedEntityToRepository(session, fileSystemAccessor.GetFileName(fileNamesWithCachedEntity),
                            this.Deserrialize(fileSystemAccessor.ReadAllText(fileNamesWithCachedEntity)));
                        fileSystemAccessor.DeleteFile(fileNamesWithCachedEntity);
                    }

                    fileNamesWithCachedEntities =
                        fileSystemAccessor.GetFilesInDirectory(fileSystemAccessor.CombinePath(basePath, cacheFolderUniqueId));
                }
            }

            this.fileSystemAccessor.DeleteDirectory(fileSystemAccessor.CombinePath(basePath, this.cacheFolderUniqueId));
        }

        private void StoreCachedEntityToRepository(BulkInsertOperation bulkOperation, string id, TEntity entity)
        {
            if(entity==null)
                return;
            string ravenId = ToRavenId(id);
            bulkOperation.Store(entity: entity, id: ravenId);
        }

        private JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    ContractResolver = ravenStore.Conventions.JsonContractResolver
                };
            }
        }

        private string GetPathToEntity(string entityName)
        {
            return fileSystemAccessor.CombinePath(fileSystemAccessor.CombinePath(basePath, cacheFolderUniqueId), entityName);
        }

        private string GetItemAsContent(object item)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, JsonSerializerSettings);
        }

        private TEntity Deserrialize(string payload)
        {
            return JsonConvert.DeserializeObject<TEntity>(payload, JsonSerializerSettings);
        }

        public void Clear()
        {
            const string DefaultIndexName = "Raven/DocumentsByEntityName";
            if (this.ravenStore.DatabaseCommands.GetIndex(DefaultIndexName) != null)
                this.ravenStore.DatabaseCommands.DeleteByIndex(DefaultIndexName, new IndexQuery() { Query = string.Format("Tag: *{0}*", global::Raven.Client.Util.Inflector.Pluralize(ViewName)) }, false).WaitForCompletion();
        }
    }
}