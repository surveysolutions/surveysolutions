using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SingleWorkspaceAccessor : IWorkspaceAccessor
    {
        private readonly WorkspaceView workspaceView;

        public SingleWorkspaceAccessor(WorkspaceView workspaceView)
        {
            this.workspaceView = workspaceView;
        }

        public string GetCurrentWorkspaceName() => workspaceView.Name;

        public WorkspaceView GetCurrent() => workspaceView;
    }
}