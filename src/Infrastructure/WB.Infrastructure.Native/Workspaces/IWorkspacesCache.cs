#nullable enable
using System.Collections.Generic;

namespace WB.Infrastructure.Native.Workspaces
{
    public interface IWorkspacesCache
    {
        /// <summary>
        /// Return list of enabled workspaces
        /// </summary>
        /// <returns></returns>
        List<WorkspaceContext> AllEnabledWorkspaces();
        void InvalidateCache();

        int Revision();
    }
}
