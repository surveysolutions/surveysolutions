using System.Linq;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class WorkspaceAccessor : IWorkspaceAccessor
    {
        private readonly IPrincipal principal;
        private readonly IWorkspaceService workspaceService;

        public WorkspaceAccessor(IPrincipal principal, IWorkspaceService workspaceService)
        {
            this.principal = principal;
            this.workspaceService = workspaceService;
        }

        public WorkspaceView GetCurrent()
        {
            var currentWorkspace = principal.CurrentUserIdentity.Workspace;
            var workspace = workspaceService.GetByName(currentWorkspace);
            if (workspace != null)
                return workspace;
            return workspaceService.GetAll().FirstOrDefault();
        }
    }
}