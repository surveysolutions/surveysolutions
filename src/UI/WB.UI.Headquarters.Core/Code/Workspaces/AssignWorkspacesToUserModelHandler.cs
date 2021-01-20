using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MediatR;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class AssignWorkspacesToUserModelHandler : IRequestHandler<AssignWorkspacesToUserModelRequest>
    {
        private readonly IPlainStorageAccessor<Workspace> workspaces;
        private readonly IUserRepository users;
        private readonly IWorkspacesService workspacesService;

        public AssignWorkspacesToUserModelHandler(IPlainStorageAccessor<Workspace> workspaces,
            IUserRepository users,
            IWorkspacesService workspacesService)
        {
            this.workspaces = workspaces;
            this.users = users;
            this.workspacesService = workspacesService;
        }

        public async Task<Unit> Handle(AssignWorkspacesToUserModelRequest request,
            CancellationToken cancellationToken = default)
        {
            var model = request.AssignModel;
            var ModelState = request.ModelState;

            List<Workspace> dbWorkspaces = new();

            foreach (var modelWorkspace in model.Workspaces)
            {
                var workspace = this.workspaces.GetById(modelWorkspace);
                if (workspace == null)
                {
                    ModelState.AddModelError(nameof(model.Workspaces), $"Workspace '{modelWorkspace}' not found");
                }
                else
                {
                    dbWorkspaces.Add(workspace);
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
                    && !user.IsInRole(UserRoles.ApiUser)
                    && !user.IsInRole(UserRoles.Observer))
                {
                    ModelState.AddModelError(nameof(model.UserIds),
                        "Only headquarter or api user workspaces can be edited");
                }

                if (ModelState.IsValid)
                {
                    switch (request.AssignModel.Mode)
                    {
                        case AssignWorkspacesMode.Assign:
                            workspacesService.AssignWorkspaces(user, dbWorkspaces);
                            break;
                        case AssignWorkspacesMode.Add:
                        {
                            var resultWorkspaces = user.Workspaces.Select(u => u.Workspace).Union(dbWorkspaces);
                            workspacesService.AssignWorkspaces(user, resultWorkspaces.ToList());
                            break;
                        }
                        case AssignWorkspacesMode.Remove:
                        {
                            var resultWorkspaces = user.Workspaces.Select(u => u.Workspace).Except(dbWorkspaces);
                            workspacesService.AssignWorkspaces(user, resultWorkspaces.ToList());
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return Unit.Value;
        }
    }
}
