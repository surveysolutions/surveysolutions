using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.WebTester.Services
{
    public class WebTesterWorkspaceContextAccessor : IWorkspaceContextAccessor
    {
        public WorkspaceContext? CurrentWorkspace()
        {
            return new WorkspaceContext(WorkspaceConstants.DefaultWorkspaceName, "");
        }
    }
}
