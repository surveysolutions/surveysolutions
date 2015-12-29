using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using Humanizer;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal abstract class PostgresKeyValueStorageWithCache<TEntity> : PostgresKeyValueStorage<TEntity>
        where TEntity: class
    {
        MemoryCache memoryCache;
        readonly string cacheKey = "K/V memory cache";

        public PostgresKeyValueStorageWithCache(string connectionString, ILogger logger)
            : base(connectionString, logger)
        {
            memoryCache = new MemoryCache(cacheKey);
        }


        public override TEntity GetById(string id)
        {
            var value = this.memoryCache.Get(id) as TEntity;
            if (value != null)
                return value;

            value = base.GetById(id);
            if (value != null)
                this.memoryCache.Add(id, value, DateTimeOffset.Now.AddSeconds(30));

            return value;
        }

        public override void Remove(string id)
        {
            this.memoryCache.Remove(id);
            base.Remove(id);
        }

        public override void Store(TEntity view, string id)
        {
            this.memoryCache.Remove(id);
            base.Store(view, id);
        }

        public override void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            bulk.ForEach(i => this.memoryCache.Remove(i.Item2));
            base.BulkStore(bulk);
        }

        public override void Clear()
        {
            this.memoryCache.Dispose();
            this.memoryCache = new MemoryCache(cacheKey);
            base.Clear();
        }


        public override string GetReadableStatus()
        {
            return "Postgres with Cache K/V :/";
        }
    }
}