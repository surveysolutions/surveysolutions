using System.Linq;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class WorkspaceAccessor : IWorkspaceAccessor
    {
        private readonly IPrincipal principal;

        public WorkspaceAccessor(IPrincipal principal)
        {
            this.principal = principal;
        }

        public string GetCurrentWorkspaceName()
        {
            var currentWorkspace = principal.CurrentUserIdentity?.Workspace;
            return currentWorkspace;
        }
    }
}