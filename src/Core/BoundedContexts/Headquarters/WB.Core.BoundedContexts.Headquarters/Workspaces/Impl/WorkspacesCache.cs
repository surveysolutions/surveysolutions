#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspacesCache : IWorkspacesCache
    {
        // Needed for proper cache invalidation, because IMemoryCache is workspace dependent
        private static readonly IMemoryCache MemoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IInScopeExecutor inScopeExecutor;
        
        private const string WorkspacesCacheKey = "workspaces";

        public WorkspacesCache(IInScopeExecutor serviceLocator)
        {
            this.inScopeExecutor = serviceLocator;
        }

        public List<WorkspaceContext> AllEnabledWorkspaces()
        {
            return MemoryCache.GetOrCreateWithDoubleLock(WorkspacesCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                
                return inScopeExecutor.Execute(serviceLocator =>
                {
                    var ws = serviceLocator.GetInstance<IWorkspacesService>();
                    var list = ws.GetEnabledWorkspaces()?.ToList() ?? new List<WorkspaceContext>();
                    return list;
                });
            });
        }

        public void InvalidateCache()
        {
            MemoryCache.Remove(WorkspacesCacheKey);
        }
    }
}
