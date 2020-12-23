#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspacesCache : IWorkspacesCache
    {
        // Needed for proper cache invalidation, because IMemoryCache is workspace dependent
        private static readonly IMemoryCache MemoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IInScopeExecutor inScopeExecutor;
        
        private const string WorkspacesCacheKey = "workspaces";
        private const string WorkspacesCacheKeyAll = "workspacesall";

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
                    var list = ws.GetEnabledWorkspaces().ToList();
                    return list;
                });
            });
        }

        public List<WorkspaceContext> AllWorkspaces()
        {
            return MemoryCache.GetOrCreateWithDoubleLock(WorkspacesCacheKeyAll, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                
                return inScopeExecutor.Execute(serviceLocator =>
                {
                    var ws = serviceLocator.GetInstance<IPlainStorageAccessor<Workspace>>();
                    var list = ws.Query(_ => _.ToList()).Select(x => new WorkspaceContext(x.Name, x.DisplayName, x.DisabledAtUtc))
                                 .ToList();
                    return list;
                });
            });
        }

        public void InvalidateCache()
        {
            MemoryCache.Remove(WorkspacesCacheKey);
            MemoryCache.Remove(WorkspacesCacheKeyAll);
        }
    }
}
