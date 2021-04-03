#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Domain;
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
        private readonly IAuthorizedUser authorizedUser;
        private readonly IPlainStorageAccessor<Workspace> workspaces;
        private readonly IPlainStorageAccessor<WorkspacesUsers> workspaceUsers;
        private readonly ILogger<WorkspacesService> logger;
        private readonly IUserRepository userRepository;
        private readonly ISystemLog systemLog;
        private readonly IWorkspacesUsersCache usersCache;
        private readonly IInScopeExecutor inScopeExecutor;

        public WorkspacesService(UnitOfWorkConnectionSettings connectionSettings,
            ILoggerProvider loggerProvider,
            IAuthorizedUser authorizedUser,
            IPlainStorageAccessor<Workspace> workspaces,
            IPlainStorageAccessor<WorkspacesUsers> workspaceUsers,
            IUserRepository userRepository,
            ILogger<WorkspacesService> logger,
            ISystemLog systemLog,
            IWorkspacesUsersCache usersCache,
            IInScopeExecutor inScopeExecutor)
        {
            this.connectionSettings = connectionSettings;
            this.loggerProvider = loggerProvider;
            this.authorizedUser = authorizedUser;
            this.workspaces = workspaces;
            this.workspaceUsers = workspaceUsers;
            this.logger = logger;
            this.systemLog = systemLog;
            this.usersCache = usersCache;
            this.inScopeExecutor = inScopeExecutor;
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

        public async Task AssignWorkspacesAsync(HqUser user, List<AssignUserWorkspace> workspacesList)
        {
            List<string> workspacesRemoved = new();

            foreach (var userWorkspace in user.Workspaces.ToList())
            {
                var workspaceExists = workspacesList.Any(w => Equals(w.Workspace, userWorkspace.Workspace));

                if (!workspaceExists)
                {
                    var pendingWork = this.GetWorkAssignedToUser(user, userWorkspace.Workspace);
                    if (pendingWork.AssignmentsCount > 0 || pendingWork.InterviewsCount > 0 || pendingWork.AssignedInterviewers > 0)
                    {
                        throw new WorkspaceRemovalNotAllowedException(pendingWork.InterviewsCount, pendingWork.AssignmentsCount, pendingWork.AssignedInterviewers);
                    }

                    await ReassignPendingFromFromUser(user, userWorkspace);

                    this.workspaceUsers.Remove(userWorkspace.Id);
                    user.Workspaces.Remove(userWorkspace);
                    workspacesRemoved.Add(userWorkspace.Workspace.Name);
                    this.logger.LogInformation("Removed {user} from {workspace} by {currentUser}", user.UserName, userWorkspace.Workspace.Name, this.authorizedUser.UserName);
                }
            }

            if (workspacesRemoved.Any())
            {
                this.systemLog.WorkspaceUserUnAssigned(user.UserName, workspacesRemoved);
            }

            List<string> workspacesAdded = new();

            foreach (var workspace in workspacesList)
            {
                var workspaceExists = user.Workspaces.Any(uw => Equals(uw.Workspace, workspace.Workspace));

                if (!workspaceExists)
                {
                    AddUserToWorkspace(user, workspace.Workspace.Name, workspace.SupervisorId);
                    workspacesAdded.Add(workspace.Workspace.Name);
                }
            }

            if (workspacesAdded.Any())
            {
                this.systemLog.WorkspaceUserAssigned(user.UserName, workspacesAdded);
            }

            this.usersCache.Invalidate(user.Id);
        }

        private async Task ReassignPendingFromFromUser(HqUser user, WorkspacesUsers userWorkspace)
        {
            if (user.IsInRole(UserRoles.Interviewer))
            {
                await this.inScopeExecutor.ExecuteAsync(async sl =>
                {
                    var scopedUsers = sl.GetInstance<IUserRepository>();
                    var moveService = sl.GetInstance<IMoveUserToAnotherTeamService>();

                    var scopedInterviewer = await scopedUsers.FindByIdAsync(user.Id);

                    var moveResult = moveService.MoveInterviewsToSupervisor(this.authorizedUser.Id,
                        user.Id,
                        scopedInterviewer.Profile.SupervisorId.GetValueOrDefault());

                    if(moveResult.Errors.Count > 0)
                    {
                        throw new Exception("Failed to reassign interviewer tasks to other responsible");
                    }

                }, userWorkspace.Workspace.Name);
            }
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

        public void AddUserToWorkspace(HqUser user, string workspace, Guid? supervisorId)
        {
            Workspace workspaceEntity = workspaces.GetById(workspace) ??
                                        throw new ArgumentException(@"Workspace not found", nameof(workspace))
                                        {
                                            Data =
                                            {
                                                {"name", workspace}
                                            }
                                        };

            if (user.IsInRole(UserRoles.Interviewer) && !supervisorId.HasValue)
                throw new ArgumentException(@"Supervisor should be set for interviewer", nameof(supervisorId))
                {
                    Data =
                    {
                        {"supervisorId", supervisorId}
                    }
                };

            if (!user.IsInRole(UserRoles.Interviewer))
                supervisorId = null;

            var workspaceUser = new WorkspacesUsers(workspaceEntity, user, supervisorId.HasValue ? this.userRepository.FindById(supervisorId.Value) : null);

            this.workspaceUsers.Store(workspaceUser, workspaceUser.Id);

            this.logger.LogInformation("Added {user} to {workspace}", user.UserName, workspace);
        }

        private AssignedWorkInfo GetWorkAssignedToUser(HqUser user, Workspace workspace)
        {
            return this.inScopeExecutor.Execute(sl =>
            {
                var interviewInfo = sl.GetInstance<IInterviewInformationFactory>();
                var assignmentsService = sl.GetInstance<IAssignmentsService>();
                var userViewFactory = sl.GetInstance<IUserViewFactory>();

                int interviewsCount = 0;
                int assignmentsCount = 0;
                int assignedInterviewers = 0;
                if (user.IsInRole(UserRoles.Interviewer))
                {
                    interviewsCount = interviewInfo.GetInProgressInterviewsForInterviewer(user.Id).Count;
                    assignmentsCount = assignmentsService.GetAllAssignmentIds(user.Id).Count;
                }
                else if (user.IsInRole(UserRoles.Supervisor))
                {
                    interviewsCount = interviewInfo.GetInProgressInterviewsForSupervisor(user.Id).Count;
                    assignmentsCount = assignmentsService.GetAllAssignmentIds(user.Id).Count;
                    assignedInterviewers = userViewFactory.GetInterviewers(user.Id).Count();
                }

                return new AssignedWorkInfo
                {
                    InterviewsCount = interviewsCount,
                    AssignmentsCount = assignmentsCount,
                    AssignedInterviewers = assignedInterviewers
                };
            }, workspace.Name);
        }
    }
}
