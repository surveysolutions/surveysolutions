#nullable enable
using Microsoft.AspNetCore.Builder;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public static class WorkspaceExtension
    {
        public static void UseWorkspaces(this IApplicationBuilder app)
        {
            app.UseMiddleware<WorkspaceMiddleware>();
        }

        public static void UseRedirectIntoWorkspace(this IApplicationBuilder app)
        {
            app.UseMiddleware<WorkspaceRedirectMiddleware>();
        }
    }
}
