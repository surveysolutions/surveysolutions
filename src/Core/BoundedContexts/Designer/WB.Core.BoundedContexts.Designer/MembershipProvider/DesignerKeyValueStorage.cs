using System;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class DesignerKeyValueStorage<T> : IPlainKeyValueStorage<T> where T : class
    {
        private readonly DesignerDbContext dbContext;
        private readonly IEntitySerializer<T> serializer;

        public DesignerKeyValueStorage(
            DesignerDbContext dbContext,
            IEntitySerializer<T> serializer)
        {
            this.dbContext = dbContext;
            this.serializer = serializer;
        }

        public T GetById(string id)
        {
            StoredInAttribute storedInAttribute =
                (StoredInAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(StoredInAttribute));

            var byId = (KeyValueEntity)this.dbContext.Find(storedInAttribute.StoredIn, id);
            if (byId != null)
            {
                return this.serializer.Deserialize(byId.Value);
            }
            return null;
        }

        public void Remove(string id)
        {
            StoredInAttribute storedInAttribute =
                (StoredInAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(StoredInAttribute));

            var byId = (KeyValueEntity)this.dbContext.Find(storedInAttribute.StoredIn, id);
            if (byId != null)
            {
                this.dbContext.Remove(byId);
            }
        }

        public void Store(T entity, string id)
        {
            StoredInAttribute storedInAttribute =
                (StoredInAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(StoredInAttribute));

            var byId = (KeyValueEntity)this.dbContext.Find(storedInAttribute.StoredIn, id);
            if (byId != null)
            {
                byId.Value = this.serializer.Serialize(entity);
            }
            else
            {
                var store = Activator.CreateInstance<KeyValueEntity>();
                store.Id = id;
                store.Value = this.serializer.Serialize(entity);
                dbContext.Add(store);
            }
        }
    }
}
