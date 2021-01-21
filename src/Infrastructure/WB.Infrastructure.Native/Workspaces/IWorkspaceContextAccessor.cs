#nullable enable
namespace WB.Infrastructure.Native.Workspaces
{
    public interface IWorkspaceContextAccessor
    {
        WorkspaceContext? CurrentWorkspace();
    }
}
