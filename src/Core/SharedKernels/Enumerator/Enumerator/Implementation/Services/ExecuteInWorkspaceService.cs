using System;
using System.Threading.Tasks;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class ExecuteInWorkspaceService : IInScopeExecutor
    {
        private readonly ILifetimeScope lifetimeScope;

        public ExecuteInWorkspaceService(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
        }

        private ILifetimeScope CreateChildScopeWithWorkspace(string workspace)
        {
            var workspaceAccessor = new SingleWorkspaceAccessor(workspace);
            var workspaceLifetimeScope = lifetimeScope.BeginLifetimeScope(cb =>
            {
                cb.Register(c => workspaceAccessor).As<IWorkspaceAccessor>().SingleInstance();
            });

            return workspaceLifetimeScope;
        }

        public void Execute(Action<IServiceLocator> action, string workspace = null)
        {
            if (workspace == null)
            {
                action.Invoke(lifetimeScope.Resolve<IServiceLocator>());
                return;
            }
            
            using var workspaceLifetimeScope = CreateChildScopeWithWorkspace(workspace);
            var serviceLocator = workspaceLifetimeScope.Resolve<IServiceLocator>();
            action.Invoke(serviceLocator);
        }

        public T Execute<T>(Func<IServiceLocator, T> func, string workspace = null)
        {
            if (workspace == null)
                return func.Invoke(lifetimeScope.Resolve<IServiceLocator>());

            using var workspaceLifetimeScope = CreateChildScopeWithWorkspace(workspace);
            var serviceLocator = workspaceLifetimeScope.Resolve<IServiceLocator>();
            return func.Invoke(serviceLocator);
        }

        public Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func, string workspace = null)
        {
            if (workspace == null)
                return func.Invoke(lifetimeScope.Resolve<IServiceLocator>());

            using var workspaceLifetimeScope = CreateChildScopeWithWorkspace(workspace);
            var serviceLocator = workspaceLifetimeScope.Resolve<IServiceLocator>();
            return func.Invoke(serviceLocator);
        }

        public Task ExecuteAsync(Func<IServiceLocator, Task> func, string workspace = null)
        {
            if (workspace == null)
                return func.Invoke(lifetimeScope.Resolve<IServiceLocator>());
            
            using var workspaceLifetimeScope = CreateChildScopeWithWorkspace(workspace);
            var serviceLocator = workspaceLifetimeScope.Resolve<IServiceLocator>();
            return func.Invoke(serviceLocator);
        }
    }
}