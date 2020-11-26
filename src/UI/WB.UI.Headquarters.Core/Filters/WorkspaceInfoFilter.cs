using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Workspaces;

namespace WB.UI.Headquarters.Filters
{
    public class WorkspaceInfoFilter : IResultFilter
    {
        private readonly IWorkspacesService workspacesService;
        private readonly IAuthorizedUser authorizedUser;

        public WorkspaceInfoFilter(IWorkspacesService workspacesService,
            IAuthorizedUser authorizedUser)
        {
            this.workspacesService = workspacesService;
            this.authorizedUser = authorizedUser;
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated && context.Result is ViewResult view)
            {
              //  var userId = authorizedUser.Id;
                var claims = context.HttpContext.User.Claims.Where(c => c.Type == "Workspace").Select(c => c.Value).ToHashSet();
                var workspaces = this.workspacesService.GetWorkspaces();
                var userWorkspaces = workspaces.Where(w => claims.Contains(w.Name));
                view.ViewData["UserWorkspacesList"] = userWorkspaces;
            }

        }
    }
}
