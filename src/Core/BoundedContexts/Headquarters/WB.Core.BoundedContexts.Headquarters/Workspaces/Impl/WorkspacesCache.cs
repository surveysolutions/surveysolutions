using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspacesCache : IWorkspacesCache
    {
        // Needed for proper cache invalidation, because IMemoryCache is workspace dependent
        private static readonly IMemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IServiceLocator serviceLocator;

        public WorkspacesCache(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public IEnumerable<WorkspaceContext> GetWorkspaces()
        {
            return memoryCache.GetOrCreate("workspaces", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return serviceLocator.ExecuteInScope<IWorkspacesService, List<WorkspaceContext>>(ws =>
                    ws.GetWorkspaces().ToList());
            });
        }

        public void InvalidateCache()
        {
            memoryCache.Remove("workspaces");
        }
    }
}
