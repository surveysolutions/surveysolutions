#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public interface IWorkspacesService
    {
        public Task Generate(string name, DbUpgradeSettings upgradeSettings);
        bool IsWorkspaceDefined(string? workspace);
        IEnumerable<string> GetWorkspacesForUser(Guid userId);
        void AddUserToWorkspace(Guid user, string workspace);
        bool UserHasWorkspace(Guid user, string workspace);
        IEnumerable<string> GetWorkspaces();
    }
    
    class WorkspacesService : IWorkspacesService
    {
        private readonly UnitOfWorkConnectionSettings connectionSettings;
        private readonly ILoggerProvider loggerProvider;
        private readonly IPlainStorageAccessor<Workspace> workspaces;
        private readonly IPlainStorageAccessor<WorkspacesUsers> workspaceUsers;
        private readonly ILogger<WorkspacesService> logger;

        public WorkspacesService(UnitOfWorkConnectionSettings connectionSettings,
            ILoggerProvider loggerProvider, 
            IPlainStorageAccessor<Workspace> workspaces,
            IPlainStorageAccessor<WorkspacesUsers> workspaceUsers,
            ILogger<WorkspacesService> logger)
        {
            this.connectionSettings = connectionSettings;
            this.loggerProvider = loggerProvider;
            this.workspaces = workspaces;
            this.workspaceUsers = workspaceUsers;
            this.logger = logger;
        }

        public Task Generate(string name, DbUpgradeSettings upgradeSettings)
        {
            string schemaName = "ws_" + name;
            DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                schemaName);
            this.logger.LogInformation("Adding workspace {workspace}", name);
            return Task.Run(() => DbMigrationsRunner.MigrateToLatest(connectionSettings.ConnectionString,
                schemaName,
                upgradeSettings, loggerProvider));
        }

        public bool IsWorkspaceDefined(string? workspace)
        {
            return workspaces.Query(_ => _.Any(x => x.Name == workspace));
        }

        public IEnumerable<string> GetWorkspacesForUser(Guid userId)
        {
            var userWorkspaces = workspaces.Query(_ =>
                _.Where(x => x.Users.Any(u => u.UserId == userId))
                .Select(x => x.Name)
                .ToList()
            );
            return userWorkspaces;
        }

        public void AddUserToWorkspace(Guid user, string workspace)
        {
            var workspaceEntity = workspaces.GetById(workspace) ?? throw new ArgumentNullException("Workspace not found");

            var workspaceUser = new WorkspacesUsers
            {
                Workspace = workspaceEntity,
                UserId = user
            };
            
            this.workspaceUsers.Store(workspaceUser, workspaceUser.Id);
            
            this.logger.LogInformation("Added {user} to {workspace}", user, workspace);
        }

        public bool UserHasWorkspace(Guid user, string workspace)
        {
            var hasWorkspace = this.workspaceUsers.Query(_ => _.Any(x => x.Workspace.Name == workspace && x.UserId == user));
            return hasWorkspace;
        }

        public IEnumerable<string> GetWorkspaces()
        {
            return this.workspaces.Query(_ => _.OrderBy(x => x.Name).Select(x => x.Name).ToList());
        }
    }
}
