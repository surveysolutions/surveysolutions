#nullable enable
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspaceContextAccessor : IWorkspaceContextAccessor
    {
        private readonly IWorkspaceContextHolder holder;

        public WorkspaceContextAccessor(IWorkspaceContextHolder holder)
        {
            this.holder = holder;
        }

        public WorkspaceContext? CurrentWorkspace() => holder.Current;
    }
}
