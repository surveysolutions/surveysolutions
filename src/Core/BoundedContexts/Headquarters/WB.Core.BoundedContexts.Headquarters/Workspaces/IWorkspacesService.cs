#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public interface IWorkspacesService
    {
        public Task Generate(string name, DbUpgradeSettings upgradeSettings);
        bool IsWorkspaceDefined(string? workspace);
        IEnumerable<WorkspaceContext> GetWorkspacesForUser(Guid userId);
        void AddUserToWorkspace(Guid user, string workspace);
        IEnumerable<WorkspaceContext> GetWorkspaces();
    }
}
