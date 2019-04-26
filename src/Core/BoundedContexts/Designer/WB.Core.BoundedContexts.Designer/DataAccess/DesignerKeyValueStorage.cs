using System;
using Main.Core.Documents;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Designer.Aggregates;
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

        public T GetById(string id)
        {
            return memoryCache.GetOrCreate(CacheKey(id), entry =>
            {
                var storedInAttribute = GetTypeToQuery();

                var byId = (KeyValueEntity)this.dbContext.Find(storedInAttribute, id);
                if (byId != null)
                {
                    return this.serializer.Deserialize(byId.Value);
                }
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));
                
                return null;
            });            
        }

        private string CacheKey(string id) => GetTypeToQuery().FullName + id;

        private static Type GetTypeToQuery()
        {
            if (typeof(T) == typeof(QuestionnaireDocument)) return typeof(StoredQuestionnaireDocument);
            StoredInAttribute storedInAttribute =
                (StoredInAttribute) Attribute.GetCustomAttribute(typeof(T), typeof(StoredInAttribute));
            return storedInAttribute.StoredIn;
        }

        public void Remove(string id)
        {
            var byId = (KeyValueEntity)this.dbContext.Find(GetTypeToQuery(), id);
            if (byId != null)
            {
                this.memoryCache.Remove(CacheKey(id));
                this.dbContext.Remove(byId);                
            }
        }

        public void Store(T entity, string id)
        {
            memoryCache.Remove(CacheKey(id));
            var typeToQuery = GetTypeToQuery();

            var byId = (KeyValueEntity)this.dbContext.Find(typeToQuery, id);
            if (byId != null)
            {
                byId.Value = this.serializer.Serialize(entity);
            }
            else
            {
                var instance = Activator.CreateInstance(typeToQuery);
                var store = (KeyValueEntity)instance;
                store.Id = id;
                store.Value = this.serializer.Serialize(entity);
                
                dbContext.Add(instance);
            }
        }
    }
}
