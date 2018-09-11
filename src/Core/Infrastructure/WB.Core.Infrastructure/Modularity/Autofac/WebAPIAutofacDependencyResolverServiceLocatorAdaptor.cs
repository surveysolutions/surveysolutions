using System;
using System.Collections.Generic;
using Autofac.Integration.WebApi;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Modularity.Autofac
{
    public class WebAPIAutofacDependencyResolverServiceLocatorAdaptor : ServiceLocatorImplBase
    {
        private AutofacWebApiDependencyResolver resolver;

        public WebAPIAutofacDependencyResolverServiceLocatorAdaptor(AutofacWebApiDependencyResolver resolver)
        {
            this.resolver = resolver;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            return this.resolver.GetService(serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return this.resolver.GetServices(serviceType);
        }
    }
}
