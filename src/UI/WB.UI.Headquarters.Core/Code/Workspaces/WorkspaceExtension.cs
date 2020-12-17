#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using WB.Core.BoundedContexts.Headquarters.Services;
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
    }
}
