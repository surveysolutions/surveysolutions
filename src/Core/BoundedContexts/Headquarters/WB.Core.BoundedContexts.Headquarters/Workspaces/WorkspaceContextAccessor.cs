#nullable enable
using System;
using System.Linq;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public class WorkspaceContextHolder : IWorkspaceContextHolder
    {
        public WorkspaceContext? Current { get; set; }
    }

    public interface IWorkspaceContextHolder
    {
        WorkspaceContext? Current { get; set; }
    }

    class WorkspaceContextSetter : IWorkspaceContextSetter
    {
        private readonly IWorkspacesService workspacesService;
        private readonly IWorkspaceContextHolder holder;

        public WorkspaceContextSetter(IWorkspacesService workspacesService, IWorkspaceContextHolder holder)
        {
            this.workspacesService = workspacesService;
            this.holder = holder;
        }

        public void Set(string name)
        {
            var workspace = workspacesService.GetWorkspaces().FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            holder.Current = workspace ?? throw new ArgumentNullException(nameof(name));
        }
    }

    public class WorkspaceContextAccessor : IWorkspaceContextAccessor
    {
        private readonly IWorkspaceContextHolder holder;

        public WorkspaceContextAccessor(IWorkspaceContextHolder holder)
        {
            this.holder = holder;
        }

        public WorkspaceContext CurrentWorkspace() => holder.Current ?? Workspace.Default.AsContext();
    }
}
