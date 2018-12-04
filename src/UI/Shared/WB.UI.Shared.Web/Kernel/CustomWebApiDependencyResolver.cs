using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Integration.WebApi;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Shared.Web.Kernel
{
    /// <summary>
    /// Autofac implementation of the <see cref="T:System.Web.Http.Dependencies.IDependencyResolver" /> interface.
    /// </summary>
    public class CustomWebApiDependencyResolver : AutofacWebApiDependencyResolver
    {
        /// <summary>Try to get a service of the given type.</summary>
        /// <param name="serviceType">Type of service to request.</param>
        /// <returns>An instance of the service, or null if the service is not found.</returns>
        public new object GetService(Type serviceType)
        {
            var scope = (BeginScope() as AutofacWebApiDependencyScope).LifetimeScope;
            var serviceLocator = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));

            return base.GetService(serviceType);
        }

        public new IEnumerable<object> GetServices(Type serviceType)
        {
            var scope = (BeginScope() as AutofacWebApiDependencyScope).LifetimeScope;
            var serviceLocator = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));

            return base.GetServices(serviceType);
        }

        public CustomWebApiDependencyResolver(ILifetimeScope container) : base(container)
        {
        }
    }
}
