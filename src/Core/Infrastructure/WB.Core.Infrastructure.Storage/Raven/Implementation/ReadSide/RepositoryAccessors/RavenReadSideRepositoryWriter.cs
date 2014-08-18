﻿using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    #warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryWriter<TEntity> : RavenReadSideRepositoryAccessor<TEntity>, IQueryableReadSideRepositoryWriter<TEntity>, IRavenReadSideRepositoryWriter
        where TEntity : class, IReadSideRepositoryEntity
    {
        private const int MaxCountOfCachedEntities = 256;
        private const int MaxCountOfEntitiesInOneStoreOperation = 16;

        private class CachedEntity
        {
            public CachedEntity(TEntity entity, bool shouldBeStoredToRepository)
            {
                this.Entity = entity;
                this.ShouldBeStoredToRepository = shouldBeStoredToRepository;
            }

            public TEntity Entity { get; private set; }

            public bool ShouldBeStoredToRepository { get; set; }
        }

        private bool isCacheEnabled = false;
        private readonly Dictionary<string, CachedEntity> cache = new Dictionary<string, CachedEntity>();

        public RavenReadSideRepositoryWriter(DocumentStore ravenStore, IRavenReadSideRepositoryWriterRegistry writerRegistry)
            : base(ravenStore)
        {
            writerRegistry.Register(this);
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
                        .Customize(customization => customization.WaitForNonStaleResultsAsOfNow()));
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
        }

        public void DisableCache()
        {
            this.StoreAllCachedEntitiesToRepository();
            this.ClearCache();

            this.isCacheEnabled = false;
        }

        public string GetReadableStatus()
        {
            int cachedEntities = this.cache.Count;
            int cachedEntitiesWhichNeedToBeStoredToRepository = this.cache.Count(entity => entity.Value.ShouldBeStoredToRepository);

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
            if (!this.cache.ContainsKey(id))
            {
                this.ReduceCacheIfNeeded();

                TEntity entity = this.GetByIdAvoidingCache(id);

                this.cache.Add(id, new CachedEntity(entity, shouldBeStoredToRepository: false));
            }

            return this.cache[id].Entity;
        }

        private void RemoveUsingCache(string id)
        {
            if (this.cache.ContainsKey(id))
            {
                this.cache.Remove(id);
            }
            this.RemoveAvoidingCache(id);
        }

        private void StoreUsingCache(TEntity entity, string id)
        {
            if (this.cache.ContainsKey(id))
            {
                this.cache[id] = new CachedEntity(entity, shouldBeStoredToRepository: true);
            }
            else
            {
                this.ReduceCacheIfNeeded();

                this.cache.Add(id, new CachedEntity(entity, shouldBeStoredToRepository: true));
            }
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
            List<KeyValuePair<string, CachedEntity>> bulk = this.GetBulkOfEntitiesForRemovanceFromCache();

            this.StoreBulkOfCachedEntitiesToRepository(bulk);

            this.RemoveEntitiesFromCache(bulk.Select(cachedEntityWithId => cachedEntityWithId.Key));
        }

        private List<KeyValuePair<string, CachedEntity>> GetBulkOfEntitiesForRemovanceFromCache()
        {
            return this.cache.Take(MaxCountOfEntitiesInOneStoreOperation).ToList();
        }

        private void RemoveEntitiesFromCache(IEnumerable<string> ids)
        {
            foreach (string id in ids)
            {
                this.cache.Remove(id);
            }
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= MaxCountOfCachedEntities;
        }

        private void StoreAllCachedEntitiesToRepository()
        {
            var bulkOfCachedEntities = new List<KeyValuePair<string, CachedEntity>>(MaxCountOfEntitiesInOneStoreOperation);

            foreach (KeyValuePair<string, CachedEntity> cachedEntityWithId in this.cache)
            {
                if (cachedEntityWithId.Value.ShouldBeStoredToRepository)
                {
                    bulkOfCachedEntities.Add(cachedEntityWithId);

                    bool isBulkFull = bulkOfCachedEntities.Count >= MaxCountOfEntitiesInOneStoreOperation;
                    if (isBulkFull)
                    {
                        this.StoreBulkOfCachedEntitiesToRepository(bulkOfCachedEntities);
                        bulkOfCachedEntities.Clear();
                    }
                }
            }

            if (bulkOfCachedEntities.Count > 0)
            {
                this.StoreBulkOfCachedEntitiesToRepository(bulkOfCachedEntities);
            }
        }

        private void StoreBulkOfCachedEntitiesToRepository(List<KeyValuePair<string, CachedEntity>> bulkOfCachedEntities)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                foreach (KeyValuePair<string, CachedEntity> cachedEntityWithId in bulkOfCachedEntities)
                {
                    if (cachedEntityWithId.Value.ShouldBeStoredToRepository)
                    {
                        string ravenId = ToRavenId(cachedEntityWithId.Key);
                        session.Store(entity: cachedEntityWithId.Value.Entity, id: ravenId);
                    }
                }

                session.SaveChanges();
            }

            foreach (KeyValuePair<string, CachedEntity> cachedEntityWithId in bulkOfCachedEntities)
            {
                cachedEntityWithId.Value.ShouldBeStoredToRepository = false;
            }
        }

        private void ClearCache()
        {
            this.cache.Clear();
        }
    }
}