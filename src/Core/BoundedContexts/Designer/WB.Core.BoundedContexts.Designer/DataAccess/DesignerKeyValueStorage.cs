using System;
using System.Diagnostics;
using Main.Core.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
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

        public DesignerKeyValueStorage(
            DesignerDbContext dbContext,
            IMemoryCache memoryCache,
            IEntitySerializer<T> serializer)
        {
            this.dbContext = dbContext;
            this.memoryCache = memoryCache;
            this.serializer = serializer;
        }

        public T? GetById(string id)
        {
            return memoryCache.GetOrCreate(CacheKey(id), cache =>
            {
                cache.SetSlidingExpiration(TimeSpan.FromMinutes(5));

                var entry = FindEntry(id);

                if (entry != null)
                {
                    if (entry.State == EntityState.Deleted)
                    {
                        return null;
                    }

                    return this.serializer.Deserialize(entry.Entity.Value);
                }

                return null;
            });            
        }

        public bool HasNotEmptyValue(string id)
        {
            var entity = FindEntry(id);

            return entity != null && entity.State != EntityState.Deleted;
        }

        public void Remove(string id)
        {
            var entity = FindEntry(id);

            if(entity != null && entity.State != EntityState.Deleted)
            {
                this.memoryCache.Remove(CacheKey(id));
                this.dbContext.Remove(entity.Entity);
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
                var store = (KeyValueEntity)instance;
                store.Id = id;
                store.Value = this.serializer.Serialize(entity);
                
                dbContext.Add(instance);
            }

            memoryCache.Remove(CacheKey(id));
            GetById(id);
        }

        private string CacheKey(string id) => QueryType.Name + id;

        private static Type? _queryType = null;
        private static Type QueryType
        {
            get
            {
                if (_queryType == null)
                {
                    if (typeof(T) == typeof(QuestionnaireDocument)) return typeof(StoredQuestionnaireDocument);
                    StoredInAttribute storedInAttribute =
                        (StoredInAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(StoredInAttribute));

                    _queryType = storedInAttribute.StoredIn;
                }

                return _queryType;
            }
        }

    }
}
