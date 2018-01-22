using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InMemoryCacheStorage<T, TK> : IDisposable, ICacheStorage<T, TK> where T : class
    {
        private readonly ConcurrentDictionary<Guid, Dictionary<TK, T>> memoryCache =
            new ConcurrentDictionary<Guid, Dictionary<TK, T>>();

        private readonly IDisposable evictionNotification;

        public InMemoryCacheStorage(IEvictionObservable evictionNotification)
        {
            this.evictionNotification = evictionNotification.Subscribe(RemoveArea);
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

        public void Dispose()
        {
            evictionNotification?.Dispose();
        }
    }
}