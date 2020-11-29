using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspacesCache : IWorkspacesCache
    {
        private readonly IMemoryCache memoryCache;
        private readonly IServiceLocator serviceLocator;

        public WorkspacesCache(IMemoryCache memoryCache, IServiceLocator serviceLocator)
        {
            this.memoryCache = memoryCache;
            this.serviceLocator = serviceLocator;
        }

        public IEnumerable<WorkspaceContext> GetWorkspaces()
        {
            return memoryCache.GetOrCreate("workspaces", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return serviceLocator.ExecuteInScope<IWorkspacesService, List<WorkspaceContext>>(ws =>
                    Enumerable.ToList<WorkspaceContext>(ws.GetWorkspaces()));
            });
        }

        public void InvalidateCache()
        {
            memoryCache.Remove("workspaces");
        }
    }
}