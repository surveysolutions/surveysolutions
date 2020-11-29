using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
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
            var workspacesService = serviceLocator.GetInstance<IWorkspacesCache>();
            var workspace = workspacesService.GetWorkspaces().FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            holder.Current = workspace ?? throw new ArgumentNullException(nameof(name));
        }
    }
}