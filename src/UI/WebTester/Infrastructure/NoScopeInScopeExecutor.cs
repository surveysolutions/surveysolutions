using System;
using System.Threading.Tasks;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.WebTester.Infrastructure
{
    public class NoScopeInScopeExecutor : IInScopeExecutor
    {
        private readonly ILifetimeScope lifetimeScope;

        public NoScopeInScopeExecutor(ILifetimeScope rootScope)
        {
            lifetimeScope = rootScope;
        }

        public void Execute(Action<IServiceLocator> action)
        {
            action(lifetimeScope.Resolve<IServiceLocator>());
        }

        public T Execute<T>(Func<IServiceLocator, T> func)
        {
            return func(lifetimeScope.Resolve<IServiceLocator>());
        }

        public Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func)
        {
            return func(lifetimeScope.Resolve<IServiceLocator>());
        }

        public Task ExecuteAsync(Func<IServiceLocator, Task> func)
        {
            return func(lifetimeScope.Resolve<IServiceLocator>());
        }
    }
}
