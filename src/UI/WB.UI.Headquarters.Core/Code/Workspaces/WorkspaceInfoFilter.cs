using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceInfoFilter : IResultFilter
    {
        private readonly IWorkspacesCache workspacesService;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;
        private readonly IDataProtector dataProtector;

        public const string CookieName = "currentWorkspace";
        
        public WorkspaceInfoFilter(IWorkspacesCache workspacesService,
            IWorkspaceContextAccessor workspaceContextAccessor,
            IDataProtectionProvider protectionProvider)
        {
            this.workspacesService = workspacesService;
            this.workspaceContextAccessor = workspaceContextAccessor;
            this.dataProtector = protectionProvider.CreateProtector("ws_cookie");
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if(context.Controller is UnderConstructionController) return;

            if (context.HttpContext.User.Identity.IsAuthenticated && context.Result is ViewResult view)
            {
                view.ViewData["UserWorkspacesList"] = this.workspacesService.CurrentUserWorkspaces();

                var currentWorkspace = workspaceContextAccessor.CurrentWorkspace();

                if (currentWorkspace != null)
                {
                    var payload = dataProtector.Protect(currentWorkspace.Name);

                    context.HttpContext.Response.Cookies.Append(CookieName,
                        payload,
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.Now.AddDays(7)
                        });
                }
            }
        }
    }
}
