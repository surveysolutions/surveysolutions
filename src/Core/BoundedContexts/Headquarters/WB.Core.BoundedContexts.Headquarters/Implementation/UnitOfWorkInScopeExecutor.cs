using System;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;

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
            if(lifetimeScope == null) throw new Exception($"Class was not initialized");
            return lifetimeScope.BeginLifetimeScope(AutofacServiceLocatorConstants.UnitOfWorkScope);
        }

        public void ExecuteActionInScope(Action<IServiceLocator> action)
        {
            using (var scope = CreateChildContainer())
            {
                var serviceLocatorLocal = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));

                action(serviceLocatorLocal);

                serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();
            }
        }

        public bool ExecuteFunctionInScope(Func<IServiceLocator, bool> func)
        {
            using (var scope = CreateChildContainer())
            {
                var serviceLocatorLocal = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));

                var result = func(serviceLocatorLocal);

                serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();

                return result;
            }
        }
    }
}
