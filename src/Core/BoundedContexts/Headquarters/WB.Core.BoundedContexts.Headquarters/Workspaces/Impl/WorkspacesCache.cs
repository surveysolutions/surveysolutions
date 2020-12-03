#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspacesCache : IWorkspacesCache
    {
        // Needed for proper cache invalidation, because IMemoryCache is workspace dependent
        private static readonly IMemoryCache MemoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IServiceLocator serviceLocator;
        private readonly IAuthorizedUser authorizedUser;

        public WorkspacesCache(IServiceLocator serviceLocator,
            IAuthorizedUser authorizedUser)
        {
            this.serviceLocator = serviceLocator;
            this.authorizedUser = authorizedUser;
        }

        public IEnumerable<WorkspaceContext> AllWorkspaces()
        {
            return MemoryCache.GetOrCreate("workspaces", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return serviceLocator.ExecuteInScope<IWorkspacesService, List<WorkspaceContext>>(ws =>
                    ws.GetWorkspaces().ToList());
            });
        }

        public IEnumerable<WorkspaceContext> CurrentUserWorkspaces()
        {
            var workspaceNames = this.authorizedUser.Workspaces;
            var workspaces = this.AllWorkspaces();
            var userWorkspaces = workspaces.Where(w => workspaceNames.Contains(w.Name));
            return userWorkspaces;
        }

        public void InvalidateCache()
        {
            MemoryCache.Remove("workspaces");
        }
    }
}
