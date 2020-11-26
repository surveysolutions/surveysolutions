using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WB.Infrastructure.Native.Workspaces
{
    // singleton
    public class WorkspaceAwareMemoryCache : IMemoryCacheSource
    {
        readonly ConcurrentDictionary<string, IMemoryCache> caches = new ConcurrentDictionary<string, IMemoryCache>(); 
        
        public IMemoryCache GetCache(string workspace)
        {
            return caches.GetOrAdd(workspace, _ => new MemoryCache(Options.Create(new MemoryCacheOptions())));
        }
    }
}
