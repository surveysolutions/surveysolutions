using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Criterion;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize]
    public class WorkspaceTypeaheadController : ControllerBase
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IPlainStorageAccessor<Workspace> workspaces;

        public WorkspaceTypeaheadController(
            IAuthorizedUser authorizedUser,
            IPlainStorageAccessor<Workspace> workspaces)
        {
            this.authorizedUser = authorizedUser;
            this.workspaces = workspaces;
        }

        [HttpGet]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
        public TypeaheadApiView<string> Workspaces(string query, bool includeDisabled = false, int limit = 10)
        {
            var result =
                this.workspaces.Query(_ =>
                        Filter(query, includeDisabled, _)
                            .Take(limit)
                            .ToList())
                    .Select(x => new TypeaheadOptionalApiView<string>()
                    {
                        key = x.Name,
                        value = x.DisplayName,
                        iconClass = x.DisabledAtUtc != null ? "disabled-item" : null
                    }).ToList();
            int totalCount = this.workspaces.Query(_ => Filter(query, includeDisabled, _).Count());

            return new TypeaheadApiView<string>
            (
                1,
                result.Count,
                totalCount,
                result,
                null
            );
        }
        
        private IQueryable<Workspace> Filter(string query, bool includeDisabled, IQueryable<Workspace> source)
        {
            IQueryable<Workspace> result = source.OrderBy(x => x.DisplayName);

            if (!this.authorizedUser.IsAdministrator)
            {
                var userWorkspaces = this.authorizedUser.Workspaces.ToList();
                result = result.Where(x => userWorkspaces.Contains(x.Name));
            }

            if (!string.IsNullOrEmpty(query))
            {
                var lowerCaseQuery = query.ToLower();
                result = result.Where(w => w.DisplayName.ToLower().Contains(lowerCaseQuery));
            }

            if (!includeDisabled)
            {
                result = result.Where(w => w.DisabledAtUtc == null);
            }

            return result;
        }
    }
}