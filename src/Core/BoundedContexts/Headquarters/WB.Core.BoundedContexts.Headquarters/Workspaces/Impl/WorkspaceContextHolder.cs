#nullable enable
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    public class WorkspaceContextHolder : IWorkspaceContextHolder
    {
        public WorkspaceContext? Current { get; set; }
    }
}
