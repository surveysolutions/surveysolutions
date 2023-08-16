﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MediatR;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Impl;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class AssignWorkspacesToUserModelHandler : IRequestHandler<AssignWorkspacesToUserModelRequest, Unit>
    {
        private readonly IWorkspacesStorage workspaces;
        private readonly IUserRepository users;
        private readonly IWorkspacesService workspacesService;
        private readonly IWorkspacesUsersCache workspacesUsers;
        private readonly IAuthorizedUser authorizedUser;

        public AssignWorkspacesToUserModelHandler(IWorkspacesStorage workspaces,
            IUserRepository users,
            IWorkspacesService workspacesService,
            IWorkspacesUsersCache workspacesUsers,
            IAuthorizedUser authorizedUser)
        {
            this.workspaces = workspaces;
            this.users = users;
            this.workspacesService = workspacesService;
            this.workspacesUsers = workspacesUsers;
            this.authorizedUser = authorizedUser;
        }

        public async Task<Unit> Handle(AssignWorkspacesToUserModelRequest request,
            CancellationToken cancellationToken = default)
        {
            var model = request.AssignModel;
            var ModelState = request.ModelState;

            var accessibleWorkspaces = authorizedUser.IsAdministrator
                ? workspacesService.GetAllWorkspaces().Select(w => w.Name).ToList()
                : await workspacesUsers.GetUserWorkspaces(authorizedUser.Id, cancellationToken);
            
            List<AssignUserWorkspace> dbWorkspaces = new();

            foreach (var modelWorkspace in model.Workspaces)
            {
                var workspace = this.workspaces.GetById(modelWorkspace.Workspace);
                var hasAccess = accessibleWorkspaces.Contains(modelWorkspace.Workspace);
                if (workspace == null || !hasAccess)
                {
                    ModelState.AddModelError(nameof(model.Workspaces), $"Workspace '{modelWorkspace.Workspace}' not found");
                }
                else
                {
                    dbWorkspaces.Add(new AssignUserWorkspace() { Workspace = workspace, SupervisorId = modelWorkspace.SupervisorId});
                }
            }

            foreach (var userId in model.UserIds)
            {
                var user = await users.FindByIdAsync(userId, cancellationToken);

                if (user == null)
                {
                    ModelState.AddModelError(nameof(model.UserIds), "User not found");
                    continue;
                }

                if (user.IsArchivedOrLocked)
                {
                    ModelState.AddModelError(nameof(model.UserIds), "User is locked");
                }
                
                if (!user.IsInRole(UserRoles.Headquarter)
                    && !user.IsInRole(UserRoles.Supervisor)
                    && !user.IsInRole(UserRoles.Interviewer)
                    && !user.IsInRole(UserRoles.ApiUser)
                    && !user.IsInRole(UserRoles.Observer))
                {
                    ModelState.AddModelError(nameof(model.UserIds),
                        "Only headquarter, supervisor, interviewer, observer or api user workspaces can be edited");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        switch (request.AssignModel.Mode)
                        {
                            case AssignWorkspacesMode.Assign:
                            {
                                var hiddenForUserWorkspaces = workspacesService.GetAllWorkspaces()
                                    .Where(w => !accessibleWorkspaces.Contains(w.Name))
                                    .Select(w => w.Name).ToList();
                                var hiddenUserWorkspaces = user.Workspaces
                                    .Where(w => hiddenForUserWorkspaces.Contains(w.Workspace.Name))
                                    .Select(u => new AssignUserWorkspace()
                                    {
                                        Workspace = u.Workspace,
                                        SupervisorId = u.Supervisor?.Id
                                    });
                                var userWorkspacesWithHidden = dbWorkspaces.Concat(hiddenUserWorkspaces).ToList();
                                await workspacesService.AssignWorkspacesAsync(user, userWorkspacesWithHidden);
                                break;
                            }
                            case AssignWorkspacesMode.Add:
                            {
                                var userWorkspaces = user.Workspaces.Select(u => u.Workspace);
                                var newWorkspaces = dbWorkspaces.Where(aw => !userWorkspaces.Contains(aw.Workspace));
                                var resultWorkspaces = user.Workspaces.Select(u => new AssignUserWorkspace()
                                {
                                    Workspace = u.Workspace,
                                    SupervisorId = u.Supervisor?.Id
                                }).Concat(newWorkspaces);
                                await workspacesService.AssignWorkspacesAsync(user, resultWorkspaces.ToList());
                                break;
                            }
                            case AssignWorkspacesMode.Remove:
                            {
                                var workspacesToRemove = dbWorkspaces.Select(d => d.Workspace);
                                var workspacesAfterRemove = user.Workspaces.Where(uw => !workspacesToRemove.Contains(uw.Workspace));
                                var resultWorkspaces = workspacesAfterRemove.Select(u => new AssignUserWorkspace()
                                {
                                    Workspace = u.Workspace,
                                    SupervisorId = u.Supervisor?.Id
                                });
                                await workspacesService.AssignWorkspacesAsync(user, resultWorkspaces.ToList());
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch (WorkspaceRemovalNotAllowedException e)
                    {
                        var errorMessage =
                            string.Format(Resources.Workspaces.WorkspaceCantBeRemoved, e.InterviewsCount, e.AssignmentsCount, e.InterviewersCount);
                        ModelState.AddModelError(nameof(model.UserIds), errorMessage);
                    }
                }
            }

            return Unit.Value;
        }
    }
}
