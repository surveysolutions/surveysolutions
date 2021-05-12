using System;
using System.Threading.Tasks;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class UnitOfWorkInScopeExecutor<TService> : UnitOfWorkInScopeExecutor, IInScopeExecutor<TService>
    {
        public UnitOfWorkInScopeExecutor(ILifetimeScope rootScope) : base(rootScope)
        {
        }

        public void Execute(Action<TService> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service = scope.Resolve<TService>();
            action(service);
            scope.Resolve<IUnitOfWork>().AcceptChanges();
        }

        public TResult Execute<TResult>(Func<TService, TResult> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service = scope.Resolve<TService>();
            var result = action(service);
            scope.Resolve<IUnitOfWork>().AcceptChanges();
            return result;
        }

        public async Task ExecuteAsync(Func<TService, Task> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service = scope.Resolve<TService>();
            await action(service);
            scope.Resolve<IUnitOfWork>().AcceptChanges();
        }        
        
        public async Task<T> ExecuteAsync<T>(Func<TService, Task<T>> action, string workspace)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service = scope.Resolve<TService>();
            var res = await action(service);
            scope.Resolve<IUnitOfWork>().AcceptChanges();
            return res;
        }
    }

    public class UnitOfWorkInScopeExecutor<TService1, TService2> 
        : UnitOfWorkInScopeExecutor, IInScopeExecutor<TService1, TService2>
    {
        public UnitOfWorkInScopeExecutor(ILifetimeScope rootScope) : base(rootScope)
        {
        }

        public void Execute(Action<TService1, TService2> action, string workspace)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service1 = scope.Resolve<TService1>();
            var service2 = scope.Resolve<TService2>();
            action(service1, service2);
            scope.Resolve<IUnitOfWork>().AcceptChanges();
        }

        public async Task ExecuteAsync(Func<TService1, TService2, Task> action, string workspace)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service1 = scope.Resolve<TService1>();
            var service2 = scope.Resolve<TService2>();
            await action(service1, service2);
            scope.Resolve<IUnitOfWork>().AcceptChanges();
        }

        public TResult Execute<TResult>(Func<TService1, TService2, TResult> action, string workspace)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service1 = scope.Resolve<TService1>();
            var service2 = scope.Resolve<TService2>();
            var result = action(service1, service2);
            scope.Resolve<IUnitOfWork>().AcceptChanges();
            return result;
        }
    }

    public class UnitOfWorkInScopeExecutor : IInScopeExecutor
    {
        private readonly ILifetimeScope lifetimeScope;
        private long Depth = 0;

        public UnitOfWorkInScopeExecutor(ILifetimeScope rootScope)
        {
            lifetimeScope = rootScope;

            if (rootScope.Tag is long depth)
            {
                Depth = depth;
            }
        }
        
        protected ILifetimeScope CreateChildContainer(string workspace = null)
        {
            if (lifetimeScope == null) throw new Exception($"Class was not initialized");
            
            var scope = lifetimeScope.BeginLifetimeScope(Depth + 1);

            if (workspace == null)
            {
                var currentWorkspace = lifetimeScope.Resolve<IWorkspaceContextAccessor>().CurrentWorkspace();

                if (currentWorkspace != null)
                {
                    scope.Resolve<IWorkspaceContextSetter>().Set(currentWorkspace);
                }
            }
            else
            {
                scope.Resolve<IWorkspaceContextSetter>().Set(workspace);
            }

            return scope;
        }
        
        public void Execute(Action<IServiceLocator> action, string workspace = null)
        {
            using var scope = CreateChildContainer(workspace);
            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();

            action(serviceLocatorLocal);

            serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();
        }

        public T Execute<T>(Func<IServiceLocator, T> func, string workspace = null)
        {
            using var scope = CreateChildContainer(workspace);
            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();

            var result = func(serviceLocatorLocal);

            serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();

            return result;
        }

        public async Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func, string workspace = null)
        {
            using var scope = CreateChildContainer(workspace);

            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();
            var result = await func(serviceLocatorLocal);

            scope.Resolve<IUnitOfWork>().AcceptChanges();

            return result;
        }

        public async Task ExecuteAsync(Func<IServiceLocator, Task> func, string workspace = null)
        {
            using var scope = CreateChildContainer(workspace);
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
