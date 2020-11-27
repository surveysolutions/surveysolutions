#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WB.Infrastructure.Native.Workspaces;

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

        public static IEnumerable<string> GetWorkspaceClaims(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(c => c.Type == WorkspaceConstants.ClaimType).Select(c => c.Value);
        }
    }
}
