using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Integration.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Shared.Web.Kernel
{
    public class CustomMVCDependencyResolver : Autofac.Integration.Mvc.AutofacDependencyResolver
    {
        public CustomMVCDependencyResolver(ILifetimeScope container) : base(container)
        {
        }

        public CustomMVCDependencyResolver(ILifetimeScope container, Action<ContainerBuilder> configurationAction) : base(container, configurationAction)
        {
        }

        public CustomMVCDependencyResolver(ILifetimeScope container, ILifetimeScopeProvider lifetimeScopeProvider) : base(container, lifetimeScopeProvider)
        {
        }

        public CustomMVCDependencyResolver(ILifetimeScope container, ILifetimeScopeProvider lifetimeScopeProvider, Action<ContainerBuilder> configurationAction) : base(container, lifetimeScopeProvider, configurationAction)
        {
        }

        public override object GetService(Type serviceType)
        {
            //to preserve scope
            var serviceLocator = this.RequestLifetimeScope.Resolve<IServiceLocator>(new NamedParameter("kernel", this.RequestLifetimeScope));
            return base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            var serviceLocator = this.RequestLifetimeScope.Resolve<IServiceLocator>(new NamedParameter("kernel", this.RequestLifetimeScope));
            return base.GetServices(serviceType);
        }
    }
}
