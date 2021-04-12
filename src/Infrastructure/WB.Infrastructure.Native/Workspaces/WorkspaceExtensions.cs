#nullable enable
using System;
using System.Collections.Generic;
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

        public static bool IsSystemDefinedWorkspace(this WorkspaceContext? context)
        {
            return context != null && WorkspaceConstants.IsSystemDefinedWorkspace(context.Name);
        }   
        
        public static bool IsAdministrationWorkspace(this WorkspaceContext? context)
        {
            return context?.Name == WorkspaceConstants.WorkspaceNames.AdminWorkspaceName;
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

        public static WorkspaceContext? GetWorkspaceContext(this IServiceProvider provider)
        {
            return provider.GetRequiredService<IWorkspaceContextAccessor>().CurrentWorkspace();
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
    }
}
