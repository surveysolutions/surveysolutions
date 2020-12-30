using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Domain
{
    public class NoScopeInScopeExecutor<T>: IInScopeExecutor<T>
    {
        private readonly T service;

        public NoScopeInScopeExecutor(T service)
        {
            this.service = service;
        }
        
        public void Execute(Action<T> action, string workspace = null)
        {
            action(service);
        }

        public TResult Execute<TResult>(Func<T, TResult> action, string workspace = null)
        {
            return action(service);
        }

        public Task ExecuteAsync(Func<T, Task> action, string workspace)
        {
            action(service);
            return Task.CompletedTask;
        }

        public Task<T1> ExecuteAsync<T1>(Func<T, Task<T1>> action, string workspace = null)
        {
            return action(service);
        }
    } 
    
    public class NoScopeInScopeExecutor<T1, T2>: IInScopeExecutor<T1, T2>
    {
        private readonly T1 service1;
        private readonly T2 service2;

        public NoScopeInScopeExecutor(T1 service1, T2 service2)
        {
            this.service1 = service1;
            this.service2 = service2;
        }
        
        public void Execute(Action<T1, T2> action, string workspace = null)
        {
            action(service1, service2);
        }

        public TResult Execute<TResult>(Func<T1, T2, TResult> action, string workspace = null)
        {
            return action(service1, service2);
        }

        public Task ExecuteAsync(Func<T1, T2, Task> action, string workspace)
        {
            action(service1, service2);
            return Task.CompletedTask;
        }
    }

    public class NoScopeInScopeExecutor : IInScopeExecutor, IRootScopeExecutor
    {
        private readonly IServiceLocator lifetimeScope;

        public NoScopeInScopeExecutor(IServiceLocator rootScope)
        {
            lifetimeScope = rootScope;
        }

        public void Execute(Action<IServiceLocator> action, string workspace = null)
        {
            action(lifetimeScope);
        }

        public T Execute<T>(Func<IServiceLocator, T> func)
        {
            return func(lifetimeScope);
        }

        public Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func, string workspace = null)
        {
            return func(lifetimeScope);
        }

        public Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func)
        {
            return func(lifetimeScope);
        }

        public Task ExecuteAsync(Func<IServiceLocator, Task> func, string workspace = null)
        {
            return func(lifetimeScope);
        }
    }
}
