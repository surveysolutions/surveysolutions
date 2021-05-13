using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WB.Core.SharedKernels.Enumerator.Services.Workspace
{
    public class WorkspaceMemoryCacheSource : IWorkspaceMemoryCacheSource
    {
        readonly ConcurrentDictionary<string, IMemoryCache> caches = new ConcurrentDictionary<string, IMemoryCache>(); 
        
        public IMemoryCache GetCache(string workspace)
        {
            return caches.GetOrAdd(workspace, _ => new MemoryCache(Options.Create(new MemoryCacheOptions())));
        }

        public void ClearAll()
        {
            caches.Clear();
        }
    }
}