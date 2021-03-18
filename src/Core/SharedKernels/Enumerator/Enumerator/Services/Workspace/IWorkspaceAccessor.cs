using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services.Workspace
{
    public interface IWorkspaceAccessor
    {
        string GetCurrentWorkspaceName();
        WorkspaceView GetCurrent();
    }
}