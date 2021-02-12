using System;
using System.Linq;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspaceContextSetter : IWorkspaceContextSetter
    {
        private readonly IWorkspaceContextHolder holder;
        private readonly IWorkspacesCache workspacesService;
        
        public WorkspaceContextSetter(
            IWorkspaceContextHolder holder, 
            IWorkspacesCache workspacesService)
        {
            this.holder = holder;
            this.workspacesService = workspacesService;
        }

        public void Set(WorkspaceContext workspace)
        {
            holder.Current = workspace;
        }

        public void Set(string name)
        {
            var workspace = name == WorkspaceConstants.AdminWorkspaceName 
                ? Workspace.Admin.AsContext() 
                : name == WorkspaceConstants.UsersWorkspaceName 
                    ? Workspace.UsersWorkspace.AsContext() 
                    : workspacesService.AllWorkspaces()
                        .FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            
            holder.Current = workspace ?? throw new MissingWorkspaceException { Data = {{"name", name}}};
        }
    }
}
