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

        private IServiceLocator CreateChildScopeWithWorkspace(string workspace)
        {
            var workspaceAccessor = new SingleWorkspaceAccessor(workspace);
            using var workspaceLifetimeScope = lifetimeScope.BeginLifetimeScope(cb =>
            {
                cb.Register(c => workspaceAccessor).As<IWorkspaceAccessor>().SingleInstance();
                cb.RegisterType<AutofacServiceLocatorAdapter>().As<IServiceLocator>();
            });

            var workspaceServiceLocator = workspaceLifetimeScope.Resolve<IServiceLocator>();
            return workspaceServiceLocator;
        }

        public void Execute(Action<IServiceLocator> action, string workspace = null)
        {
            var serviceLocator = CreateChildScopeWithWorkspace(workspace);
            action.Invoke(serviceLocator);
        }

        public T Execute<T>(Func<IServiceLocator, T> func, string workspace = null)
        {
            var serviceLocator = CreateChildScopeWithWorkspace(workspace);
            return func.Invoke(serviceLocator);
        }

        public Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func, string workspace = null)
        {
            var serviceLocator = CreateChildScopeWithWorkspace(workspace);
            return func.Invoke(serviceLocator);
        }

        public Task ExecuteAsync(Func<IServiceLocator, Task> func, string workspace = null)
        {
            var serviceLocator = CreateChildScopeWithWorkspace(workspace);
            return func.Invoke(serviceLocator);
        }
    }
}