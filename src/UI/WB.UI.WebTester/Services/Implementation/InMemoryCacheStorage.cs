using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InMemoryCacheStorage<T, TK> : ICacheStorage<T, TK> where T : class
    {
        private readonly ConcurrentDictionary<Guid, Dictionary<TK, T>> memoryCache =
            new ConcurrentDictionary<Guid, Dictionary<TK, T>>();

        public IEnumerable<T> GetArea(Guid area)
        {
            memoryCache.TryGetValue(area, out var cache);

            return (IEnumerable<T>)cache?.Values ?? Array.Empty<T>();
        }

        public void Remove(TK id, Guid area = default(Guid))
        {
            if (!memoryCache.TryGetValue(area, out var cache)) return;

            cache.Remove(id);
        }

        public void RemoveArea(Guid area)
        {
            memoryCache.TryRemove(area, out _);
        }
        
        public void Store(T entity, TK entityKey, Guid area = default(Guid))
        {
            memoryCache.AddOrUpdate(area, key => new Dictionary<TK, T>
            {
                [entityKey] = entity
            }, (key, cache) =>
            {
                cache[entityKey] = entity; return cache;
            });
        }

        public T Get(TK id, Guid area = default(Guid))
        {
            if (!memoryCache.TryGetValue(area, out var cache)) return null;

            return cache.TryGetValue(id, out var file) ? file : null;
        }
    }
}