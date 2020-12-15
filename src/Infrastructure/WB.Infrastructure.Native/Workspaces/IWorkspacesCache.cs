#nullable enable
using System.Collections.Generic;

namespace WB.Infrastructure.Native.Workspaces
{
    public interface IWorkspacesCache
    {
        List<WorkspaceContext> AllEnabledWorkspaces();

        IEnumerable<WorkspaceContext> AllCurrentUserWorkspaces();

        void InvalidateCache();
        
        bool IsWorkspaceAccessAllowedForCurrentUser(string targetWorkspace);
    }
}
