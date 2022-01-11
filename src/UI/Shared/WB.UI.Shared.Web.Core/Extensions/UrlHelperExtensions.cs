using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Shared.Web.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string Static(this IUrlHelper helper, string path)
        {
            var contextAccessor = helper.ActionContext.HttpContext.RequestServices.GetRequiredService<IWorkspaceContextAccessor>();
            var workspace = contextAccessor.CurrentWorkspace();

            return workspace?.PathBase + path;
        }

        public static string ContentAtWorkspace(this IUrlHelper helper, WorkspaceContext workspaceContext, 
            string path)
        {
            return workspaceContext.PathBase + "/" + workspaceContext.Name + "/" + path.TrimStart('/');
        }

        public static string ActionAtWorkspace(this IUrlHelper helper, WorkspaceContext workspaceContext, 
            string action, string controller)
        {
            return workspaceContext.PathBase + "/" + workspaceContext.Name + "/" + helper.Action(action, controller);
        }
    }
}
