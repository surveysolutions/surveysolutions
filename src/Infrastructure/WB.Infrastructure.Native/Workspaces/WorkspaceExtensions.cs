#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;

namespace WB.Infrastructure.Native.Workspaces
{
    public static class WorkspaceExtensions
    {
        public static IServiceScope CreateWorkspaceScope(this IServiceProvider provider, WorkspaceContext? workspace = null)
        {
            var scope = provider.CreateScope();

            if (workspace == null)
            {
                var workspaceAccessor = provider.GetService<IWorkspaceContextAccessor>();
                workspace = workspaceAccessor?.CurrentWorkspace();
            }

            if (workspace != null)
            {
                scope.ServiceProvider.GetService<IWorkspaceContextSetter>()?.Set(workspace);
            }

            return scope;
        }
    }
}
