﻿using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal abstract class PostgresKeyValueStorageWithCache<TEntity> : PostgresKeyValueStorage<TEntity>
        where TEntity : class
    {
        private readonly object lockObject = new object();

        static MemoryCache memoryCache = new MemoryCache(typeof(TEntity).Name + " K/V memory cache");

        public PostgresKeyValueStorageWithCache(string connectionString, ILogger logger) : base(connectionString, logger)
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
                base.Remove(id);
                memoryCache.Remove(id);
            }
        }

        public override void Store(TEntity view, string id)
        {
            lock (lockObject)
            {
                base.Store(view, id);
                memoryCache.Set(id, view, DateTimeOffset.Now.AddSeconds(10));
            }
        }

        public override void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            lock (lockObject)
            {
                base.BulkStore(bulk);
                bulk.ForEach(i => memoryCache.Remove(i.Item2));
            }
        }

        public override void Clear()
        {
            lock (lockObject)
            {
                base.Clear();
                memoryCache.Dispose();
                memoryCache = new MemoryCache(typeof(TEntity).Name + " K/V memory cache");
            }
        }


        public override string GetReadableStatus()
        {
            return "Postgres with Cache K/V :/";
        }
    }
}