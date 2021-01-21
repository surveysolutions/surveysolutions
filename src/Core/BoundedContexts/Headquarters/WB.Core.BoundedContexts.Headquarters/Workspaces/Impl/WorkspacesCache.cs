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
        private readonly IInScopeExecutor<IWorkspacesService> inScopeExecutor;
        
        private const string WorkspacesCacheKey = "workspaces";

        public WorkspacesCache(IInScopeExecutor<IWorkspacesService> serviceLocator)
        {
            this.inScopeExecutor = serviceLocator;
        }

        public WorkspaceContext? GetWorkspace(string workspace)
        {
            return AllWorkspaces().SingleOrDefault(w => w.Name.Equals(workspace, StringComparison.OrdinalIgnoreCase));
        }

        public List<WorkspaceContext> AllEnabledWorkspaces() => GetAllWorkspaces().EnabledWorkspaces;

        public List<WorkspaceContext> AllWorkspaces() => GetAllWorkspaces().AllWorkspaces;

        private CacheItem GetAllWorkspaces()
        {
            return MemoryCache.GetOrCreateWithDoubleLock(WorkspacesCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                return inScopeExecutor.Execute(ws =>
                {
                    var workspaces = ws.GetAllWorkspaces()?.ToList() ?? new List<WorkspaceContext>();
                    var enabledWorkspaces = new List<WorkspaceContext>();
                    int revision = 17;

                    foreach (var workspace in workspaces)
                    {
                        if (workspace.DisabledAtUtc == null)
                        {
                            enabledWorkspaces.Add(workspace);
                        }

                        revision = revision * 23 + workspace.GetHashCode();
                    }

                    return new CacheItem(revision, workspaces, enabledWorkspaces);
                });
            });
        }

        public void InvalidateCache() => MemoryCache.Remove(WorkspacesCacheKey);

        public int Revision() => GetAllWorkspaces().Revision;

        internal class CacheItem
        {
            public CacheItem(int revision, 
                List<WorkspaceContext> allWorkspaces, 
                List<WorkspaceContext> enabledWorkspaces)
            {
                Revision = revision;
                AllWorkspaces = allWorkspaces;
                EnabledWorkspaces = enabledWorkspaces;
            }

            public int Revision { get; }
            public List<WorkspaceContext> AllWorkspaces { get;  }
            public List<WorkspaceContext> EnabledWorkspaces { get; set; }
        }
    }
}
