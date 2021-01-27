#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspacesService : IWorkspacesService
    {
        private readonly UnitOfWorkConnectionSettings connectionSettings;
        private readonly ILoggerProvider loggerProvider;
        private readonly IPlainStorageAccessor<Workspace> workspaces;
        private readonly IPlainStorageAccessor<WorkspacesUsers> workspaceUsers;
        private readonly ILogger<WorkspacesService> logger;
        private readonly IUserRepository userRepository;
        private readonly ISystemLog systemLog;
        private readonly IWorkspacesUsersCache usersCache;

        public WorkspacesService(UnitOfWorkConnectionSettings connectionSettings,
            ILoggerProvider loggerProvider, 
            IPlainStorageAccessor<Workspace> workspaces,
            IPlainStorageAccessor<WorkspacesUsers> workspaceUsers,
            IUserRepository userRepository,
            ILogger<WorkspacesService> logger, 
            ISystemLog systemLog, IWorkspacesUsersCache usersCache)
        {
            this.connectionSettings = connectionSettings;
            this.loggerProvider = loggerProvider;
            this.workspaces = workspaces;
            this.workspaceUsers = workspaceUsers;
            this.logger = logger;
            this.systemLog = systemLog;
            this.usersCache = usersCache;
            this.userRepository = userRepository;
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

        public List<WorkspaceContext> GetEnabledWorkspaces()
        {
            return workspaces.Query(_ => _
                .Where(x => x.DisabledAtUtc == null)
                .Select(workspace => workspace.AsContext())
                .ToList());
        }

        public List<WorkspaceContext> GetAllWorkspaces()
        {
            return workspaces.Query(_ => _.Select(w => w.AsContext()).ToList());
        }

        public void AssignWorkspaces(HqUser user, List<Workspace> workspacesList)
        {
            List<string> workspacesRemoved = new ();

            foreach (var userWorkspace in user.Workspaces.ToList())
            {
                var workspaceExists = workspacesList.Any(w => Equals(w, userWorkspace.Workspace));
                
                if(!workspaceExists)
                {
                    this.workspaceUsers.Remove(userWorkspace.Id);
                    user.Workspaces.Remove(userWorkspace);
                    workspacesRemoved.Add(userWorkspace.Workspace.Name);
                    this.logger.LogInformation("Removed {user} from {workspace}", user.UserName, userWorkspace.Workspace.Name);
                }
            }

            if (workspacesRemoved.Any())
            {
                this.systemLog.WorkspaceUserUnAssigned(user.UserName, workspacesRemoved);
            }

            List<string> workspacesAdded = new();

            foreach (var workspace in workspacesList)
            {
                var workspaceExists = user.Workspaces.Any(uw => Equals(uw.Workspace, workspace));
                
                if (!workspaceExists)
                {
                    AddUserToWorkspace(user, workspace.Name);
                    workspacesAdded.Add(workspace.Name);
                }
            }

            if (workspacesAdded.Any())
            {
                this.systemLog.WorkspaceUserAssigned(user.UserName, workspacesAdded);
            }

            this.usersCache.Invalidate(user.Id);
        }

        public async Task DeleteAsync(WorkspaceContext workspace, CancellationToken token = default)
        {
            if (workspace.Name == WorkspaceConstants.DefaultWorkspaceName)
            {
                return;
            }
            
            var selectedRoleId = new[] { UserRoles.Interviewer, UserRoles.Supervisor }
                .Select(x => x.ToUserId()).ToArray();

            var usersToDelete = await this.workspaceUsers
                .Query(u =>
                    u.Where(w => 
                            w.Workspace.Name == workspace.Name
                            && w.User.Roles.Any(r => selectedRoleId.Contains(r.Id)))
                    .ToListAsync(cancellationToken: token));

            logger.LogWarning("Deleting workspace {name} from workspaces table", workspace.Name);
            
            this.workspaceUsers.Remove(w => w.Where(x => x.Workspace.Name == workspace.Name));
            this.workspaces.Remove(workspace.Name);
            
            logger.LogWarning("Deleting interviewers and supervisors in workspace {name}", workspace.Name);

            var orphanedUsers = usersToDelete.Select(u => u.User.Id);
            
            await this.userRepository.Users.Where(u => orphanedUsers.Contains(u.Id))
                .DeleteAsync(cancellationToken: token);
        }

        public void AddUserToWorkspace(HqUser user, string workspace)
        {
            Workspace workspaceEntity = workspaces.GetById(workspace) ?? 
                                        throw new ArgumentException(@"Workspace not found", nameof(workspace))
                                        {
                                            Data =
                                            {
                                                {"name", workspace}
                                            }
                                        };
            
            var workspaceUser = new WorkspacesUsers(workspaceEntity, user);
            
            this.workspaceUsers.Store(workspaceUser, workspaceUser.Id);
            
            this.logger.LogInformation("Added {user} to {workspace}", user.UserName, workspace);
        }
    }
}
