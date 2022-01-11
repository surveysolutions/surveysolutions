#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.UsersManagement;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
    public class UsersManagementController : Controller
    {
        private readonly IMediator mediator;
        private readonly IAuthorizedUser authorizedUser;

        public UsersManagementController(IMediator mediator, IAuthorizedUser authorizedUser)
        {
            this.mediator = mediator;
            this.authorizedUser = authorizedUser;
        }

        [ActivePage(MenuItem.UsersManagement)]
        public IActionResult Index() => View(new
        {
            CreateUrl = Url.Action("Create", "Users"),
            WorkspacesUrl = Url.Action("Workspaces", "WorkspaceTypeahead"),
            SupervisorWorkspaceUrl = Url.Action("WorkspaceSupervisors", "UsersTypeahead"),
            ArchiveUsersUrl = Url.Action("ArchiveUsers", "UsersApi"),
            MoveUserToAnotherTeamUrl = Url.Action("MoveUserToAnotherTeam", "UsersApi"),
            
            CanAddUsers = authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter,
            CanArchiveUnarchive = authorizedUser.IsAdministrator,
            CanArchiveMoveToOtherTeam = authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter,
            CanAddRemoveWorkspaces = authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter,
            
            ArchiveStatuses = new[]
            {
                new ComboboxViewItem() { Key = "false", Value = Pages.Users_ActiveUsers   },
                new ComboboxViewItem() { Key = "true",  Value = Pages.Users_ArchivedUsers },
            },
            Roles = GetRolesForFiltering(),
            Filters = GetFilters(),
        });

        private ComboboxViewItem[] GetFilters()
        {
            var items = new List<ComboboxViewItem>();

            void addFilter(UserManagementFilter filter, string title)
                => items.Add(new ComboboxViewItem() {Key = filter.ToString(), Value = title});

            if (authorizedUser.IsAdministrator)
            {
                addFilter(UserManagementFilter.WithMissingWorkspace, Users.Filter_WithMissingWorkspace);
                addFilter(UserManagementFilter.WithDisabledWorkspaces, Users.Filter_WithDisabledWorkspaces);
            }

            addFilter(UserManagementFilter.Locked, Users.Filter_Locked);
            return items.ToArray();
        }

        private ComboboxViewItem[] GetRolesForFiltering()
        {
            if (authorizedUser.IsSupervisor)
                return new ComboboxViewItem[0];
            
            var items = new List<ComboboxViewItem>();
            
            void addUserRole(UserRoles useRole)
                => items.Add(new ComboboxViewItem()
                {
                    Key = useRole.ToString(), 
                    Value = useRole.ToUiString()
                });

            addUserRole(UserRoles.Interviewer);
            addUserRole(UserRoles.Supervisor);

            if (authorizedUser.IsAdministrator)
            {
                addUserRole(UserRoles.Headquarter);
                addUserRole(UserRoles.Observer);
                addUserRole(UserRoles.ApiUser);
            }

            return items.ToArray();
        }


        public async Task<DataTableResponse<UserManagementListItem>?> List(UsersManagementRequest request, CancellationToken cancellationToken)
        {
            DataTableResponse<UserManagementListItem>? result = await this.mediator.Send(request, cancellationToken);
            return result;
        }
    }
}
