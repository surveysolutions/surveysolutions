using System;
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

        public void ExecuteActionInScope(Action<IServiceLocator> action)
        {
            action(lifetimeScope.Resolve<IServiceLocator>());
        }

        public bool ExecuteFunctionInScope(Func<IServiceLocator, bool> func)
        {
            return func(lifetimeScope.Resolve<IServiceLocator>());
        }
    }
}
