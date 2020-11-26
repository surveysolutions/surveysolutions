#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public static class WorkspaceExtension
    {
        public static void UseWorkspaces(this IApplicationBuilder app)
        {
            app.UseMiddleware<WorkspaceMiddleware>();
            app.UseMiddleware<WorkspaceScopeMiddleware>();
        }

        public static void SetWorkspace(this HttpContext httpContext, WorkspaceContext workspace)
        {
            httpContext.Items["workspace"] = workspace;
        }

        public static WorkspaceContext? GetCurrentWorkspace(this HttpContext httpContext)
        {
            return (WorkspaceContext?) (httpContext.Items.TryGetValue("workspace", out var workspace) ? workspace : null);
        }

        public static void SetWorkspaceMatchPath(this HttpContext httpContext, bool matchPath)
        {
            httpContext.Items["workspace_match_path"] = matchPath;
        }

        public static bool GetWorkspaceMatchPath(this HttpContext httpContext)
        {
            return (bool)(httpContext.Items.TryGetValue("workspace_match_path", out var workspace) ? workspace : false);
        }
    }
}
