using System;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Shared.Enumerator.Services.Internals
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
    }
}
