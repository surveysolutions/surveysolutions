using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ninject.Activation.Caching;
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

        private bool isCacheEnabled = false;

        private readonly Dictionary<string, TEntity> cache = new Dictionary<string, TEntity>();
        private readonly RavenReadSideRepositoryWriterSettings settings;
        private readonly ILogger logger;

        public RavenReadSideRepositoryWriter(IDocumentStore ravenStore, ILogger logger, RavenReadSideRepositoryWriterSettings settings)
            : base(ravenStore)
        {
            this.logger = logger;
            this.settings = settings;
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
        {
            int cachedEntities = this.cache.Count;

            return string.Format("cache {0};    cached: {1};",
                this.isCacheEnabled ? "enabled" : "disabled",
                cachedEntities);
        }

        public Type ViewType
        {
            get { return typeof (TEntity); }
        }

        private TEntity GetByIdUsingCache(string id)
        {
            if (cache.ContainsKey(id))
                return cache[id];

            var entity = GetByIdAvoidingCache(id);

            cache[id] = entity;

            this.ReduceCacheIfNeeded();

            return entity;
        }

        private void RemoveUsingCache(string id)
        {
            this.cache.Remove(id);
        }

        private void StoreUsingCache(TEntity entity, string id)
        {
            this.cache[id] = entity;

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
            this.StoreBulkEntitiesToRepository(bulk);
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= MaxCountOfCachedEntities;
        }

        private void StoreBulkEntitiesToRepository(IEnumerable<string> bulkOfEntityIds)
        {
            try
            {
                using (
                    var session =
                        this.RavenStore.BulkInsert(options:
                            new BulkInsertOptions() {OverwriteExisting = true, BatchSize = settings.BulkInsertBatchSize})
                    )
                {
                    foreach (var entityId in bulkOfEntityIds)
                    {
                        StoreCachedEntityToRepository(session, entityId, cache[entityId]);
                    }
                }

                foreach (var entityId in bulkOfEntityIds)
                {
                    cache.Remove(entityId);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        private void StoreCachedEntityToRepository(BulkInsertOperation bulkOperation, string id, TEntity entity)
        {
            if(entity==null)
                return;
            string ravenId = ToRavenId(id);
            bulkOperation.Store(entity: entity, id: ravenId);
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

        protected override TResult QueryImpl<TResult>(Func<IRavenQueryable<TEntity>, TResult> query)
        {
            throw new NotImplementedException();
        }
    }
}