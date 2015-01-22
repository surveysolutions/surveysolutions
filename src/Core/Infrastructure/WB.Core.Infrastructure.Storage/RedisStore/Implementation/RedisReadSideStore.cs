using System;
using Humanizer;
using ServiceStack.Redis;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

using Raven.Imports.Newtonsoft.Json;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;

namespace WB.Core.Infrastructure.Storage.RedisStore.Implementation
{
    public class RedisReadSideStore<TEntity> : IReadSideKeyValueStorage<TEntity>, IReadSideRepositoryWriter, IReadSideRepositoryCleaner, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IRedisClientsManager redisClientsManager;
        private readonly string collectionName = typeof (TEntity).Name;

        public RedisReadSideStore(IRedisClientsManager redisClientsManager)
        {
            if (redisClientsManager == null) throw new ArgumentNullException("redisClientsManager");
            this.redisClientsManager = redisClientsManager;
        }

        public TEntity GetById(string id)
        {
            using (IRedisClient readOnlyClient = redisClientsManager.GetReadOnlyClient())
            {
                string valueFromHash = readOnlyClient.GetValueFromHash(collectionName, id);
                return valueFromHash == null ? null : Deserrialize(valueFromHash);
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
                string data = GetItemAsContent(view);
                redisClient.SetEntryInHash(collectionName, id, data);
            }
        }

        public string GetReadableStatus()
        {
            return "Redis :)";
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

        private JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ContractResolver = new PropertiesOnlyContractResolver(),
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
            }
        }

        private string GetItemAsContent(TEntity item)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, JsonSerializerSettings);
        }

        private TEntity Deserrialize(string payload)
        {
            try
            {
                return JsonConvert.DeserializeObject<TEntity>(payload, JsonSerializerSettings);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(payload, e);
            }
        }
    }
}