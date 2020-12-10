using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Domain
{
    public class NoScopeInScopeExecutor : IInScopeExecutor, IRootScopeExecutor
    {
        private readonly IServiceLocator lifetimeScope;

        public NoScopeInScopeExecutor(IServiceLocator rootScope)
        {
            lifetimeScope = rootScope;
        }

        public void Execute(Action<IServiceLocator> action)
        {
            action(lifetimeScope);
        }

        public T Execute<T>(Func<IServiceLocator, T> func)
        {
            return func(lifetimeScope);
        }

        public Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func)
        {
            return func(lifetimeScope);
        }

        public Task ExecuteAsync(Func<IServiceLocator, Task> func)
        {
            return func(lifetimeScope);
        }
    }
}
