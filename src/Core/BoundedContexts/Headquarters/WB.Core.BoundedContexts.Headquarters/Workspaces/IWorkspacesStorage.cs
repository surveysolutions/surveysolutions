using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces;

public interface IWorkspacesStorage
{
    Workspace GetById(string workspace);
    Task<Workspace> GetByIdAsync(string workspace);
    void Delete(string name);
    List<WorkspaceContext> GetEnabledWorkspaces();
    List<WorkspaceContext> GetAllWorkspaces();
    void Store(Workspace workspace);
    WorkspacesFilterResult FilterWorkspaces(WorkspacesFilter filter);
    bool HasWorkspaceWithName(string name);
}
