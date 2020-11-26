using WB.Infrastructure.Native.Workspaces;

#nullable enable
namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public interface IWorkspaceContextSetter
    {
        void Set(string name);
    }
}
