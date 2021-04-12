using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SingleWorkspaceAccessor : IWorkspaceAccessor
    {
        private readonly string workspace;

        public SingleWorkspaceAccessor(string workspace)
        {
            this.workspace = workspace;
        }

        public string GetCurrentWorkspaceName() => workspace;
    }
}