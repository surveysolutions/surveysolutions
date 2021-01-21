#nullable enable
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    interface IWorkspaceContextHolder
    {
        WorkspaceContext? Current { get; set; }
    }
}
