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
        private readonly object lockObject = new object();

        static MemoryCache memoryCache = new MemoryCache(typeof(TEntity).Name + " K/V memory cache");

        public PostgresKeyValueStorageWithCache(string connectionString, ILogger logger)
            : base(connectionString, logger)
        {
        }


        public override TEntity GetById(string id)
        {
            lock (lockObject)
            {
                var value = memoryCache.Get(id) as TEntity;
                if (value != null)
                    return value;

                value = base.GetById(id);
                if (value != null)
                    memoryCache.Set(id, value, DateTimeOffset.Now.AddSeconds(10));

                return value;
            }
        }

        public override void Remove(string id)
        {
            lock (lockObject)
            {
                memoryCache.Remove(id);
                base.Remove(id);
            }
        }

        public override void Store(TEntity view, string id)
        {
            lock (lockObject)
            {
                memoryCache.Set(id, view, DateTimeOffset.Now.AddSeconds(10));
                base.Store(view, id);
            }
        }

        public override void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            lock (lockObject)
            {
                bulk.ForEach(i => memoryCache.Remove(i.Item2));
                base.BulkStore(bulk);
            }
        }

        public override void Clear()
        {
            lock (lockObject)
            {
                memoryCache.Dispose();
                memoryCache = new MemoryCache(typeof (TEntity).Name + " K/V memory cache");
                base.Clear();
            }
        }


        public override string GetReadableStatus()
        {
            return "Postgres with Cache K/V :/";
        }
    }
}