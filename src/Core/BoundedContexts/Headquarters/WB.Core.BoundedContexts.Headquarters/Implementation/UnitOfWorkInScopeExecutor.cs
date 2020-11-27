using System;
using System.Threading.Tasks;
using Autofac;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class UnitOfWorkInScopeExecutor : IInScopeExecutor
    {
        private readonly ILifetimeScope lifetimeScope;

        public UnitOfWorkInScopeExecutor(ILifetimeScope rootScope)
        {
            lifetimeScope = rootScope;
        }

        public ILifetimeScope CreateChildContainer()
        {
            if (lifetimeScope == null) throw new Exception($"Class was not initialized");
            
            var scope = lifetimeScope.BeginLifetimeScope();
            var workspace = lifetimeScope.Resolve<IWorkspaceContextAccessor>().CurrentWorkspace();

            if (workspace != null)
            {
                scope.Resolve<IWorkspaceContextSetter>().Set(workspace);
            }

            return scope;
        }

        public void Execute(Action<IServiceLocator> action)
        {
            using var scope = CreateChildContainer();
            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();

            action(serviceLocatorLocal);

            serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();
        }

        public T Execute<T>(Func<IServiceLocator, T> func)
        {
            using var scope = CreateChildContainer();
            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();

            var result = func(serviceLocatorLocal);

            serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();

            return result;
        }

        public async Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func)
        {
            using var scope = CreateChildContainer();

            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();
            var result = await func(serviceLocatorLocal);

            scope.Resolve<IUnitOfWork>().AcceptChanges();

            return result;
        }

        public async Task ExecuteAsync(Func<IServiceLocator, Task> func)
        {
            using var scope = CreateChildContainer();
            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();

            await func(serviceLocatorLocal);

            scope.Resolve<IUnitOfWork>().AcceptChanges();
        }
    }

    public static class InScopeExecutorExtensions
    {
        public static async Task ExecuteAsync(this IInScopeExecutor executor,
            Func<IServiceLocator, IUnitOfWork, Task> func)
        {
            await executor.ExecuteAsync(async (locator) =>
            {
                await func(locator, locator.GetInstance<IUnitOfWork>());
            });
        }
    }
}
