using System;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Infrastructure.Native.Storage
{
    public static class AutofacServiceLocatorHelper
    {
        public static ILifetimeScope CreateChildContainer(this IServiceLocator serviceLocator,
            Action<ContainerBuilder> @override)
        {
            if (serviceLocator is AutofacServiceLocatorAdapter autofac)
            {
                return autofac.CreateChildContainer(@override);
            }

            throw new NotImplementedException();
        }

        public static ILifetimeScope CreateChildContainer(this IServiceLocator serviceLocator)
        {
            if (serviceLocator is AutofacServiceLocatorAdapter autofac)
            {
                return autofac.CreateChildContainer();
            }

            throw new NotImplementedException();
        }

        public static void ExecuteActionInScope(this IServiceLocator serviceLocator, Action<IServiceLocator> action)
        {
            using (var scope = serviceLocator.CreateChildContainer())
            {
                var serviceLocatorLocal = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));

                action(serviceLocatorLocal);

                serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();
            }
        }
        
        public static bool ExecuteFunctionInScope(this IServiceLocator serviceLocator, Func<IServiceLocator, bool> func)
        {
            using (var scope = serviceLocator.CreateChildContainer())
            {
                var serviceLocatorLocal = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));

                var result = func(serviceLocatorLocal);

                serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();

                return result;
            }
        }
    }
}
