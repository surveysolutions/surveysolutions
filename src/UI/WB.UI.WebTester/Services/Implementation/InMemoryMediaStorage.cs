using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InMemoryMediaStorage : IMediaStorage, IDisposable
    {
        private readonly IDisposable evictionNotification;

        private readonly ConcurrentDictionary<Guid, Dictionary<string, MultimediaFile>> memoryCache =
            new ConcurrentDictionary<Guid, Dictionary<string, MultimediaFile>>();
        
        public InMemoryMediaStorage(IObservable<Guid> evictionNotification)
        {
            this.evictionNotification = evictionNotification.Subscribe(key =>
            {
                memoryCache.TryRemove(key, out _);
            });
        }
        
        public void Store(Guid interviewId, MultimediaFile file)
        {
            memoryCache.AddOrUpdate(interviewId, key => new Dictionary<string, MultimediaFile>
            {
               [file.Filename] = file
            }, (key, cache) =>
            {
                cache[file.Filename] = file; return cache;
            });
        }
        
        public MultimediaFile Get(Guid interviewId, string filename)
        {
            if (!memoryCache.TryGetValue(interviewId, out var cache)) return null;

            return cache.TryGetValue(filename, out var file) ? file : null;
        }

        public void Dispose()
        {
            evictionNotification.Dispose();
        }
    }
}