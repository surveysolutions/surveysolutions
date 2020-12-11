using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class InRootScopeExecutor : IRootScopeExecutor
    {
        public static ILifetimeScope RootScope { get; set; }

        public async Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func)
        {
            if (RootScope == null)
                throw new ArgumentException("Need set root scope firstly");
            
            using var scope = RootScope.BeginLifetimeScope();
            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();

            var result = await func(serviceLocatorLocal);

            scope.Resolve<IUnitOfWork>().AcceptChanges();

            return result;
        }
    }
}