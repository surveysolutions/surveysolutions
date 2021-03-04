using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services.Workspace
{
    public interface IWorkspaceService
    {
        void Save(WorkspaceView[] workspaces);
        WorkspaceView[] GetAll();
        WorkspaceView GetByName(string workspace);
    }
}