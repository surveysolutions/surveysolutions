using System;
using System.Collections.Generic;
using System.Linq;

using Raven.Client;
using Raven.Client.Document;

using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors
{
    #warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryWriter<TEntity> : RavenReadSideRepositoryAccessor<TEntity>, IReadSideRepositoryWriter<TEntity>, IRavenReadSideRepositoryWriter
        where TEntity : class, IReadSideRepositoryEntity
    {
        private const int MaxCountOfCachedEntities = 128;
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
        private readonly Dictionary<Guid, CachedEntity> cache = new Dictionary<Guid, CachedEntity>();

        internal RavenReadSideRepositoryWriter(DocumentStore ravenStore, IRavenReadSideRepositoryWriterRegistry writerRegistry)
            : base(ravenStore)
        {
            writerRegistry.Register(this);
        }

        public TEntity GetById(Guid id)
        {
            return this.isCacheEnabled
                ? this.GetByIdUsingCache(id)
                : this.GetByIdAvoidingCache(id);
        }

        public void Remove(Guid id)
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

        public void Store(TEntity view, Guid id)
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

        private TEntity GetByIdUsingCache(Guid id)
        {
            if (!this.cache.ContainsKey(id))
            {
                if (this.IsCacheLimitReached())
                {
                    this.StoreAllCachedEntitiesToRepository();
                    this.ClearCache();
                }

                TEntity entity = this.GetByIdAvoidingCache(id);

                this.cache.Add(id, new CachedEntity(entity, shouldBeStoredToRepository: false));
            }

            return this.cache[id].Entity;
        }

        private void RemoveUsingCache(Guid id)
        {
            if (this.cache.ContainsKey(id))
            {
                this.cache.Remove(id);
            }

            this.RemoveAvoidingCache(id);
        }

        private void StoreUsingCache(TEntity entity, Guid id)
        {
            if (this.cache.ContainsKey(id))
            {
                this.cache[id] = new CachedEntity(entity, shouldBeStoredToRepository: true);
            }
            else
            {
                if (this.IsCacheLimitReached())
                {
                    this.StoreAllCachedEntitiesToRepository();
                    this.ClearCache();
                }

                this.cache.Add(id, new CachedEntity(entity, shouldBeStoredToRepository: true));
            }
        }

        private TEntity GetByIdAvoidingCache(Guid id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                return session.Load<TEntity>(id: ravenId);
            }
        }

        private void RemoveAvoidingCache(Guid id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                var view = session.Load<TEntity>(id: ravenId);

                session.Delete(view);
                session.SaveChanges();
            }
        }

        private void StoreAvoidingCache(TEntity entity, Guid id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                session.Store(entity: entity, id: ravenId);
                session.SaveChanges();
            }
        }

        private bool IsCacheLimitReached()
        {
            return this.cache.Count >= MaxCountOfCachedEntities;
        }

        private void StoreAllCachedEntitiesToRepository()
        {
            var bulkOfCachedEntities = new List<KeyValuePair<Guid, CachedEntity>>(MaxCountOfEntitiesInOneStoreOperation);

            foreach (KeyValuePair<Guid, CachedEntity> cachedEntityWithId in this.cache)
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

        private void StoreBulkOfCachedEntitiesToRepository(List<KeyValuePair<Guid, CachedEntity>> bulkOfCachedEntities)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                foreach (KeyValuePair<Guid, CachedEntity> cachedEntityWithId in bulkOfCachedEntities)
                {
                    string ravenId = ToRavenId(cachedEntityWithId.Key);

                    session.Store(entity: cachedEntityWithId.Value.Entity, id: ravenId);
                }

                session.SaveChanges();
            }

            foreach (KeyValuePair<Guid, CachedEntity> cachedEntityWithId in bulkOfCachedEntities)
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