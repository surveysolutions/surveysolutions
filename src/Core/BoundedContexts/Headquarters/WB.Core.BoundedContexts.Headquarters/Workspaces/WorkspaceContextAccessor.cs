#nullable enable
using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public class WorkspaceContextHolder : IWorkspaceContextHolder
    {
        public WorkspaceContext? Current { get; set; }
    }

     interface IWorkspaceContextHolder
    {
        WorkspaceContext? Current { get; set; }
    }

    class WorkspaceContextSetter : IWorkspaceContextSetter
    {
        private readonly IWorkspaceContextHolder holder;
        private readonly IServiceLocator serviceLocator;

        public WorkspaceContextSetter(IWorkspaceContextHolder holder, IServiceLocator serviceLocator)
        {
            this.holder = holder;
            this.serviceLocator = serviceLocator;
        }

        public void Set(WorkspaceContext workspace)
        {
            holder.Current = workspace;
        }

        public void Set(string name)
        {
            var workspacesService = serviceLocator.GetInstance<IWorkspacesService>();
            var workspace = workspacesService.GetWorkspaces().FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            holder.Current = workspace ?? throw new ArgumentNullException(nameof(name));
        }
    }

    class WorkspaceContextAccessor : IWorkspaceContextAccessor
    {
        private readonly IWorkspaceContextHolder holder;

        public WorkspaceContextAccessor(IWorkspaceContextHolder holder)
        {
            this.holder = holder;
        }

        public WorkspaceContext CurrentWorkspace()
        {
            var current = holder.Current;
            if (current != null)
            {
                return current;
            }

            var result = Workspace.Default.AsContext();
            result.UsingFallbackToDefaultWorkspace = true;
            return result;
        }
    }
}
