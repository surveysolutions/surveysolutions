using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using NpgsqlTypes;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal abstract class PostgresKeyValueStorageWithCache<TEntity> : PostgresKeyValueStorage<TEntity>
        where TEntity : class
    {
        private readonly IMemoryCache memoryCache;
        
        private static readonly string CachePrefix = $"pkvs::{typeof(TEntity).Name}::";

        public PostgresKeyValueStorageWithCache(IUnitOfWork connectionString,
            IMemoryCache memoryCache,
            IEntitySerializer<TEntity> serializer)
            : base(serializer)
        {
            this.memoryCache = memoryCache;
        }

        public override TEntity GetById(string id)
        {
            return memoryCache.GetOrCreate(CachePrefix + id, cache =>
            {
                lock (CachePrefix)
                {
                    cache.SlidingExpiration = TimeSpan.FromSeconds(10);
                    return base.GetById(id);
                }
            });
        }

        public override void Remove(string id)
        {
            try
            {
                base.Remove(id);
            }
            finally
            {
                memoryCache.Remove(CachePrefix + id);
            }
        }

        public override void Store(TEntity view, string id)
        {
            try
            {
                base.Store(view, id);
            }
            finally
            {
                memoryCache.Remove(CachePrefix + id);
            }
        }
        
        
        protected void BulkStore(List<Tuple<TEntity, string>> bulk, NpgsqlConnection connection)
        {
            using var writer = connection.BeginBinaryImport($"COPY {this.tableName}(id, value) FROM STDIN BINARY;");
            foreach (var item in bulk)
            {
                writer.StartRow();
                writer.Write(item.Item2, NpgsqlDbType.Text); // write Id
                var serializedValue = this.serializer.Serialize(item.Item1);
                writer.Write(serializedValue, NpgsqlDbType.Jsonb); // write value
                this.memoryCache.Remove(CachePrefix + item.Item2);
            }

            writer.Complete();
        }
        
        public override string GetReadableStatus()
        {
            return "Postgres with Cache K/V :/";
        }
    }
}
