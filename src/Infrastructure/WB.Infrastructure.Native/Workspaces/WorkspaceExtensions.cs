#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;

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

        public static void ExecuteInScope(this IServiceLocator serviceLocator, WorkspaceContext? workspace,
            Action<IServiceLocator> action)
        {
            var executor = serviceLocator.GetInstance<IInScopeExecutor>();

            executor.Execute(s =>
            {
                if(workspace != null)
                    s.GetInstance<IWorkspaceContextSetter>().Set(workspace);

                action(s);
            });
        } 
        
        public static async Task ExecuteInScopeAsync(this IServiceLocator serviceLocator,
            WorkspaceContext? workspace, Func<IServiceLocator, Task> action)
        {
            var executor = serviceLocator.GetInstance<IInScopeExecutor>();

            await executor.ExecuteAsync(async s=>
            {
                if(workspace != null)
                    s.GetInstance<IWorkspaceContextSetter>().Set(workspace);

                await action(s);
            });
        }
    }
}
