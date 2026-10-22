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
        public UnitOfWorkInScopeExecutor(ILifetimeScope rootScope, AmbientUnitOfWorkAccessor ambientUnitOfWorkAccessor)
            : base(rootScope, ambientUnitOfWorkAccessor)
        {
        }

        public void Execute(Action<TService> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service = scope.Resolve<TService>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);
            action(service);
            unitOfWork.AcceptChanges();
        }

        public TResult Execute<TResult>(Func<TService, TResult> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service = scope.Resolve<TService>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);
            var result = action(service);
            unitOfWork.AcceptChanges();
            return result;
        }

        public async Task ExecuteAsync(Func<TService, Task> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service = scope.Resolve<TService>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);
            await action(service);
            unitOfWork.AcceptChanges();
        }        
        
        public async Task<T> ExecuteAsync<T>(Func<TService, Task<T>> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service = scope.Resolve<TService>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);
            var res = await action(service);
            unitOfWork.AcceptChanges();
            return res;
        }
    }

    public class UnitOfWorkInScopeExecutor<TService1, TService2> 
        : UnitOfWorkInScopeExecutor, IInScopeExecutor<TService1, TService2>
    {
        public UnitOfWorkInScopeExecutor(ILifetimeScope rootScope, AmbientUnitOfWorkAccessor ambientUnitOfWorkAccessor)
            : base(rootScope, ambientUnitOfWorkAccessor)
        {
        }

        public void Execute(Action<TService1, TService2> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service1 = scope.Resolve<TService1>();
            var service2 = scope.Resolve<TService2>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);
            action(service1, service2);
            unitOfWork.AcceptChanges();
        }

        public async Task ExecuteAsync(Func<TService1, TService2, Task> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service1 = scope.Resolve<TService1>();
            var service2 = scope.Resolve<TService2>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);
            await action(service1, service2);
            unitOfWork.AcceptChanges();
        }

        public TResult Execute<TResult>(Func<TService1, TService2, TResult> action, string workspace = null)
        {
            using var scope = this.CreateChildContainer(workspace);
            var service1 = scope.Resolve<TService1>();
            var service2 = scope.Resolve<TService2>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);
            var result = action(service1, service2);
            unitOfWork.AcceptChanges();
            return result;
        }
    }

    public class UnitOfWorkInScopeExecutor : IInScopeExecutor
    {
        private readonly ILifetimeScope lifetimeScope;
        protected readonly AmbientUnitOfWorkAccessor ambientUnitOfWorkAccessor;
        private long Depth = 0;

        public UnitOfWorkInScopeExecutor(ILifetimeScope rootScope, AmbientUnitOfWorkAccessor ambientUnitOfWorkAccessor)
        {
            lifetimeScope = rootScope;
            this.ambientUnitOfWorkAccessor = ambientUnitOfWorkAccessor;

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
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);

            action(serviceLocatorLocal);

            unitOfWork.AcceptChanges();
        }

        public T Execute<T>(Func<IServiceLocator, T> func, string workspace = null)
        {
            using var scope = CreateChildContainer(workspace);
            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);

            var result = func(serviceLocatorLocal);

            unitOfWork.AcceptChanges();

            return result;
        }

        public async Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func, string workspace = null)
        {
            using var scope = CreateChildContainer(workspace);

            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);
            var result = await func(serviceLocatorLocal);

            unitOfWork.AcceptChanges();

            return result;
        }

        public async Task ExecuteAsync(Func<IServiceLocator, Task> func, string workspace = null)
        {
            using var scope = CreateChildContainer(workspace);
            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();
            var unitOfWork = scope.Resolve<IUnitOfWork>();
            using var unitOfWorkScope = this.ambientUnitOfWorkAccessor.Use(unitOfWork);

            await func(serviceLocatorLocal);

            unitOfWork.AcceptChanges();
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
