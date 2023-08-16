using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Criterion;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Impl;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize]
    public class WorkspaceTypeaheadController : ControllerBase
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IWorkspacesStorage workspaces;

        public WorkspaceTypeaheadController(
            IAuthorizedUser authorizedUser,
            IWorkspacesStorage workspaces)
        {
            this.authorizedUser = authorizedUser;
            this.workspaces = workspaces;
        }

        [HttpGet]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
        public TypeaheadApiView<string> Workspaces(string query, bool includeDisabled = false, int limit = 10)
        {
            var result = workspaces.FilterWorkspaces(new WorkspacesFilter()
            {
                Query = query,
                IncludeDisabled = includeDisabled,
                Offset = 0,
                Limit = limit,
            });

            return new TypeaheadApiView<string>
            (
                1,
                result.Workspaces.Count,
                result.TotalCount,
                result.Workspaces.Select(x => new TypeaheadOptionalApiView<string>()
                {
                    key = x.Name,
                    value = x.DisplayName,
                    iconClass = x.DisabledAtUtc != null ? "disabled-item" : null
                }).ToList(),
                null
            );
        }
    }
}