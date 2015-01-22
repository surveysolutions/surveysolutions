using System;
using Humanizer;
using ServiceStack.Redis;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.RedisStore.Implementation
{
    public class RedisReadSideStore<TEntity> : IReadSideKeyValueStorage<TEntity>, IReadSideRepositoryWriter, IReadSideRepositoryCleaner, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IRedisClientsManager redisClientsManager;
        private readonly IJsonUtils jsonUtils;
        private readonly string collectionName = typeof (TEntity).Name;

        public RedisReadSideStore(IRedisClientsManager redisClientsManager,
            IJsonUtils jsonUtils)
        {
            if (redisClientsManager == null) throw new ArgumentNullException("redisClientsManager");
            if (jsonUtils == null) throw new ArgumentNullException("jsonUtils");
            this.redisClientsManager = redisClientsManager;
            this.jsonUtils = jsonUtils;
        }

        public TEntity GetById(string id)
        {
            using (IRedisClient readOnlyClient = redisClientsManager.GetReadOnlyClient())
            {
                string valueFromHash = readOnlyClient.GetValueFromHash(collectionName, id);
                return valueFromHash == null ? null : this.jsonUtils.Deserialize<TEntity>(valueFromHash);
            }
        }

        public void Remove(string id)
        {
            using (IRedisClient redisClient = redisClientsManager.GetClient()) {
                redisClient.RemoveEntryFromHash(collectionName, id);
            }
        }

        public void Store(TEntity view, string id)
        {
            using (IRedisClient redisClient = redisClientsManager.GetClient())
            {
                string data = this.jsonUtils.Serialize(view);
                redisClient.SetEntryInHash(collectionName, id, data);
            }
        }

        public string GetReadableStatus()
        {
            return "OMG";
        }

        public Type ViewType { get { return typeof(TEntity); } }

        public void Clear()
        {
            using (IRedisClient redisClient = redisClientsManager.GetClient())
            {
                redisClient.Remove(collectionName);
            }
        }

        public void Dispose() {}

        public void EnableCache() {}

        public void DisableCache() {}
    }
}