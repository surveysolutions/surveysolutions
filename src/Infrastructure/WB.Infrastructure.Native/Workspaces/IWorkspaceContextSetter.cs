namespace WB.Infrastructure.Native.Workspaces
{
    public interface IWorkspaceContextSetter
    {
        void Set(WorkspaceContext workspace);
        void Set(string name);
    }
}