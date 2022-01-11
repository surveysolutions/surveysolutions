using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceInfoFilter : IResultFilter
    {
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IDataProtector dataProtector;

        public const string CookieName = "currentWorkspace";
        
        public WorkspaceInfoFilter(IWorkspaceContextAccessor workspaceContextAccessor,
            IDataProtectionProvider protectionProvider,
            IAuthorizedUser authorizedUser)
        {
            this.workspaceContextAccessor = workspaceContextAccessor;
            this.authorizedUser = authorizedUser;
            this.dataProtector = protectionProvider.CreateProtector("ws_cookie");
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if(context.Controller is UnderConstructionController) return;

            if (context.HttpContext.User.Identity?.IsAuthenticated == true && context.Result is ViewResult view)
            {
                view.ViewData["UserWorkspacesList"] = this.authorizedUser.GetEnabledWorkspaces();

                var currentWorkspace = workspaceContextAccessor.CurrentWorkspace();

                if (currentWorkspace != null)
                {
                    var payload = dataProtector.Protect(currentWorkspace.Name);

                    context.HttpContext.Response.Cookies.Append(CookieName,
                        payload,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTimeOffset.Now.AddDays(7)
                        });
                }
            }
        }
    }
}
