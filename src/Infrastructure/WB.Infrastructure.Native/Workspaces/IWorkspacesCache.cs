#nullable enable
using System.Collections.Generic;

namespace WB.Infrastructure.Native.Workspaces
{
    public interface IWorkspacesCache
    {
        List<WorkspaceContext> AllWorkspaces();

        IEnumerable<WorkspaceContext> CurrentUserWorkspaces();

        void InvalidateCache();
    }
}
