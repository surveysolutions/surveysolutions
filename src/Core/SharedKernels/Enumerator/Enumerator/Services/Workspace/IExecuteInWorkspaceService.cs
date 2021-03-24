using System;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services.Workspace
{
    public interface IExecuteInWorkspaceService
    {
        void Execute(WorkspaceView workspace, Action<IServiceProvider> action);
    }
}