using System;
using Main.Core.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class DesignerKeyValueStorage<T> : IPlainKeyValueStorage<T> where T : class
    {
        private readonly DesignerDbContext dbContext;
        private readonly IMemoryCache memoryCache;
        private readonly IEntitySerializer<T> serializer;
        private readonly ITransactionalMemoryCacheInvalidation cacheInvalidation;
        private readonly IKeyValueCacheEvictionTokens evictionTokens;

        public DesignerKeyValueStorage(
            DesignerDbContext dbContext,
            IMemoryCache memoryCache,
            IEntitySerializer<T> serializer,
            ITransactionalMemoryCacheInvalidation cacheInvalidation,
            IKeyValueCacheEvictionTokens evictionTokens)
        {
            this.dbContext = dbContext;
            this.memoryCache = memoryCache;
            this.serializer = serializer;
            this.cacheInvalidation = cacheInvalidation;
            this.evictionTokens = evictionTokens;
        }

        public T? GetById(string id)
        {
            var pending = FindPendingChange(id);
            if (pending != null)
            {
                // Uncommitted changes stay request-local and are never published to the shared cache.
                return pending.State == EntityState.Deleted
                    ? null
                    : this.serializer.Deserialize(pending.Entity.Value);
            }

            var storedValue = memoryCache.GetOrCreate(CacheKey(id), cache =>
            {
                // Lease the eviction source before reading the store: a concurrent commit that invalidates this key
                // while FindEntry runs cancels the token, so this entry is evicted instead of caching stale data.
                var lease = evictionTokens.Acquire(CacheKey(id));
                try
                {
                    cache.AddExpirationToken(new CancellationChangeToken(lease.Token));
                    // Release the lease when the entry is evicted so a generation is unmapped once no entry uses it.
                    cache.RegisterPostEvictionCallback((_, _, _, state) => ((ICacheEvictionLease)state!).Release(), lease);
                    cache.SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    var entry = FindEntry(id);
                    if (entry == null || entry.State == EntityState.Deleted)
                    {
                        return null;
                    }

                    return entry.Entity.Value;
                }
                catch
                {
                    // The entry is never committed, so its eviction callback won't run; release the lease here.
                    lease.Release();
                    throw;
                }
            });

            // Deserialize a fresh instance per call so callers never mutate the shared cached value.
            return storedValue == null ? null : this.serializer.Deserialize(storedValue);
        }

        // Looks only at the change tracker (no DB query) so uncommitted writes made in this request are visible locally.
        private EntityEntry<KeyValueEntity>? FindPendingChange(string id)
        {
            foreach (var entry in dbContext.ChangeTracker.Entries<KeyValueEntity>())
            {
                if (entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
                    continue;

                if (entry.Entity.GetType() == QueryType && entry.Entity.Id == id)
                    return entry;
            }

            return null;
        }

        public bool HasNotEmptyValue(string id)
        {
            var entity = FindEntry(id);

            return entity != null && entity.State != EntityState.Deleted;
        }

        public void Remove(string id)
        {
            var entity = FindEntry(id);

            if (entity != null && entity.State != EntityState.Deleted)
            {
                this.dbContext.Remove(entity.Entity);
                InvalidateCache(id);
            }
        }

        private EntityEntry<KeyValueEntity>? FindEntry(string id)
        {
            var entity = this.dbContext.Find(QueryType, id) as KeyValueEntity;
            return entity == null ? null : this.dbContext.Entry(entity);
        }

        public void Store(T entity, string id)
        {
            var entry = FindEntry(id);

            if (entry != null && entry.State != EntityState.Deleted)
            {
                entry.Entity.Value = this.serializer.Serialize(entity);
            }
            else
            {
                var instance = Activator.CreateInstance(QueryType);
                if (instance == null) throw new Exception($"Activation error of {QueryType}");

                var store = (KeyValueEntity)instance;
                store.Id = id;
                store.Value = this.serializer.Serialize(entity);
                dbContext.Add(instance);
            }

            InvalidateCache(id);
        }

        // Defer invalidation to transaction completion so a rolled-back write never leaves stale state in the shared cache.
        private void InvalidateCache(string id)
        {
            if (dbContext.Database.CurrentTransaction != null)
                cacheInvalidation.Enqueue(CacheKey(id));
            else
            {
                evictionTokens.Invalidate(CacheKey(id));
                memoryCache.Remove(CacheKey(id));
            }
        }

        private string CacheKey(string id) => QueryType.Name + id;

        private static Type? queryType = null;
        private static Type QueryType
        {
            get
            {
                if (queryType == null)
                {
                    if (typeof(T) == typeof(QuestionnaireDocument)) return typeof(StoredQuestionnaireDocument);

                    var attribute = Attribute.GetCustomAttribute(typeof(T), typeof(StoredInAttribute));
                    if (attribute == null)
                    {
                        throw new Exception($"Attribute for storage was not found for type {typeof(T).Name}");
                    }

                    StoredInAttribute storedInAttribute = (StoredInAttribute)attribute;
                    queryType = storedInAttribute.StoredIn;
                }

                return queryType;
            }
        }

    }
}
