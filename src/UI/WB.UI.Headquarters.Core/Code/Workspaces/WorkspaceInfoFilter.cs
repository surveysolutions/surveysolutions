using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Mappings;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceInfoFilter : IResultFilter
    {
        private readonly IWorkspacesCache workspacesService;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        public const string CookieName = "currentWorkspace";
        
        public WorkspaceInfoFilter(IWorkspacesCache workspacesService,
            IWorkspaceContextAccessor workspaceContextAccessor)
        {
            this.workspacesService = workspacesService;
            this.workspaceContextAccessor = workspaceContextAccessor;
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if(context.Controller is UnderConstructionController) return;

            if (context.HttpContext.User.Identity.IsAuthenticated && context.Result is ViewResult view)
            {
                var claims = context.HttpContext.User.GetWorkspaceClaims().ToHashSet();
                var workspaces = this.workspacesService.GetWorkspaces();
                var userWorkspaces = workspaces.Where(w => claims.Contains(w.Name));
                view.ViewData["UserWorkspacesList"] = userWorkspaces;

                var currentWorkspace = workspaceContextAccessor.CurrentWorkspace();

                if (currentWorkspace != null)
                {
                    context.HttpContext.Response.Cookies.Append(CookieName,
                        currentWorkspace.Name,
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.Now.AddDays(7)
                        });
                }
            }
        }
    }
}
