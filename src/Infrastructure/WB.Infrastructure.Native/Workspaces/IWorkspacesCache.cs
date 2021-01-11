#nullable enable
using System.Collections.Generic;

namespace WB.Infrastructure.Native.Workspaces
{
    public interface IWorkspacesCache
    {
        List<WorkspaceContext> AllEnabledWorkspaces();
        
        int Revision();

        List<WorkspaceContext> AllWorkspaces();

        void InvalidateCache();
    }
}
