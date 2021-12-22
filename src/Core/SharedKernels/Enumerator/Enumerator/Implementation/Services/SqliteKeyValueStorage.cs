using System;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class KeyValueItem<TKey> : IPlainStorageEntity<TKey>
    {
        public TKey Id { get; set; }
        public string Json { get; set; }
    }

    public class SqliteKeyValueStorage<TEntity> : SqliteKeyValueStorage<TEntity, string>, IPlainKeyValueStorage<TEntity>
        where TEntity : class, IPlainStorageEntity, new()
    {
        public SqliteKeyValueStorage(IPlainStorage<KeyValueItem<string>, string> fileSystemAccessor, IJsonAllTypesSerializer serializer) 
            : base(fileSystemAccessor, serializer)
        {
        }
    }

    public class SqliteKeyValueStorage<TEntity, TKey> :IDisposable, IPlainKeyValueStorage<TEntity, TKey>
        where TEntity : class, IPlainStorageEntity<TKey>, new()
    {
        private readonly IPlainStorage<KeyValueItem<TKey>, TKey> fileSystemAccessor;
        private readonly IJsonAllTypesSerializer serializer;

        public SqliteKeyValueStorage(
            IPlainStorage<KeyValueItem<TKey>, TKey> fileSystemAccessor,
            IJsonAllTypesSerializer serializer)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.serializer = serializer;
        }

        public virtual TEntity GetById(TKey id)
        {
            var keyValueItem = fileSystemAccessor.GetById(id);
            return serializer.Deserialize<TEntity>(keyValueItem.Json);
        }

        public bool HasNotEmptyValue(TKey id)
        {
            var keyValueItem = fileSystemAccessor.GetById(id);
            return keyValueItem?.Json != null;
        }

        public virtual void Remove(TKey id)
        {
            fileSystemAccessor.Remove(id);
        }

        public void Store(TEntity entity, TKey id)
        {
            var json = serializer.Serialize(entity);
            fileSystemAccessor.Store(new KeyValueItem<TKey>()
            { 
                Id = id,
                Json = json
            });
        }

        public virtual void Store(TEntity entity)
        {
            this.Store(entity, entity.Id);
        }

        public void Dispose()
        {
            this.fileSystemAccessor.Dispose();
        }
    }
}
