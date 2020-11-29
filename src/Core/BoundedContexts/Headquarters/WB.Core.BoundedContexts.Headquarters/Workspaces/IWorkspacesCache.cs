using System.Collections.Generic;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public interface IWorkspacesCache
    {
        IEnumerable<WorkspaceContext> GetWorkspaces();
        void InvalidateCache();
    }
}