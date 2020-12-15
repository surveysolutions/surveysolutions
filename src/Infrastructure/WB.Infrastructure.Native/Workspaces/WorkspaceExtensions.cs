#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;

namespace WB.Infrastructure.Native.Workspaces
{
    public static class WorkspaceExtensions
    {
        public static WorkspaceContext? CurrentWorkspace(this IServiceLocator serviceLocator)
        {
            return serviceLocator.GetInstance<IWorkspaceContextAccessor>().CurrentWorkspace();
        }

        public static void ForEachWorkspaceExecute(this IServiceLocator serviceLocator,
            Action<IServiceLocator, WorkspaceContext> action)
        {
            var workspaceCache = serviceLocator.GetInstance<IWorkspacesCache>();
            var workspaces = workspaceCache.AllEnabledWorkspaces();

            foreach (var workspace in workspaces)
            {
                serviceLocator.GetInstance<IInScopeExecutor>().Execute(scope =>
                {
                    scope.GetInstance<IWorkspaceContextSetter>().Set(workspace);
                    action(scope, workspace);
                });
            }
        }

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

        public static void ExecuteInScope(this IServiceLocator serviceLocator, WorkspaceContext? workspace,
            Action<IServiceLocator> action)
        {
            var executor = serviceLocator.GetInstance<IInScopeExecutor>();

            executor.Execute(s =>
            {
                if (workspace != null)
                    s.GetInstance<IWorkspaceContextSetter>().Set(workspace);

                action(s);
            });
        }

        public static async Task ExecuteInScopeAsync(this IServiceLocator serviceLocator,
            WorkspaceContext? workspace, Func<IServiceLocator, Task> action)
        {
            var executor = serviceLocator.GetInstance<IInScopeExecutor>();

            await executor.ExecuteAsync(async s =>
            {
                if (workspace != null)
                    s.GetInstance<IWorkspaceContextSetter>().Set(workspace);

                await action(s);
            });
        }
    }
}
