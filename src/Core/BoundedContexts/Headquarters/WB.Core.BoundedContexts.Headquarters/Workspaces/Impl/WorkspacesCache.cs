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

        public List<WorkspaceContext> AllEnabledWorkspaces()
        {
            return GetAllEnabledWorkspaces().workspaces;
        }

        private (int revision, List<WorkspaceContext> workspaces) GetAllEnabledWorkspaces()
        {
            return MemoryCache.GetOrCreateWithDoubleLock(WorkspacesCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                return inScopeExecutor.Execute(ws =>
                {
                    var list = ws.GetEnabledWorkspaces()?.ToList() ?? new List<WorkspaceContext>();

                    int revision = 17;

                    foreach (var listItem in list)
                    {
                        revision = revision * 23 + listItem.GetHashCode();
                    }

                    return (revision, list);
                });
            });
        }

        public void InvalidateCache()
        {
            MemoryCache.Remove(WorkspacesCacheKey);
        }

        public int Revision() => GetAllEnabledWorkspaces().revision;

    }
}
