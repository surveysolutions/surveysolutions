#nullable enable
using Microsoft.AspNetCore.Builder;

namespace WB.UI.Headquarters.Code.Workspace
{
    public static class WorkspaceExtension
    {
        public static void UseWorkspaces(this IApplicationBuilder app)
        {
            app.UseMiddleware<WorkspaceMiddleware>();
        }
    }
}
