using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Integration.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Modularity.Autofac
{
    public class MvcAutofacDependencyResolverServiceLocatorAdaptor : ServiceLocatorImplBase
    {
        private AutofacDependencyResolver resolver;

        public MvcAutofacDependencyResolverServiceLocatorAdaptor(AutofacDependencyResolver resolver)
        {
            this.resolver = resolver;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
