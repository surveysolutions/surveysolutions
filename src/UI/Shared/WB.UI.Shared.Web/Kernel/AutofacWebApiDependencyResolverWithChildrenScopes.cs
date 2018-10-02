using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http.Dependencies;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.WebApi;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.UI.Shared.Web.Kernel
{
/*    public class AutofacDependencyResolverWithChildrenScopes : System.Web.Http.Dependencies.IDependencyResolver
    {
        public object GetService(Type serviceType)
        {
            return ServiceLocator.Current.GetInstance(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ServiceLocator.Current.GetAllInstances(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }
    }*/

    public class AutofacDependencyResolverWithChildrenScopes : AutofacWebApiDependencyResolver
    {
        private readonly AutofacServiceLocatorAdapterWithChildrenScopes adapter;

        public AutofacDependencyResolverWithChildrenScopes(AutofacServiceLocatorAdapterWithChildrenScopes adapter, Action<ContainerBuilder> configurationAction) : base(adapter.GetRootScope(), configurationAction)
        {
            this.adapter = adapter;
        }

        public AutofacDependencyResolverWithChildrenScopes(AutofacServiceLocatorAdapterWithChildrenScopes adapter) : base(adapter.GetRootScope())
        {
            this.adapter = adapter;
        }

        public override object GetService(Type serviceType)
        {
            if (adapter.IsRegistered(serviceType))
                return adapter.GetInstance(serviceType);
            return base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            if (adapter.IsRegistered(serviceType))
                return adapter.GetAllInstances(serviceType);
            return base.GetServices(serviceType);
        }
    }


    public class AutofacWebApiDependencyResolverWithChildrenScopes : System.Web.Http.Dependencies.IDependencyResolver
    {
        private AutofacServiceLocatorAdapterWithChildrenScopes adapter;
        private AutofacWebApiDependencyResolver autofacWebApiDependencyResolver;

        public AutofacWebApiDependencyResolverWithChildrenScopes(AutofacServiceLocatorAdapterWithChildrenScopes adapter)
        {
            this.adapter = adapter;
            this.autofacWebApiDependencyResolver = new AutofacWebApiDependencyResolver(adapter.GetRootScope());
        }

        public object GetService(Type serviceType)
        {
            if (adapter.IsRegistered(serviceType))
                return adapter.GetInstance(serviceType);
            return autofacWebApiDependencyResolver.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (adapter.IsRegistered(serviceType))
                return adapter.GetAllInstances(serviceType);
            return autofacWebApiDependencyResolver.GetServices(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            //return new AutofacWebApiDependencyScope(adapter.GetCurrentScope());

            //return autofacWebApiDependencyResolver.BeginScope();
            var lifetimeScope = adapter.BeginAutofacLifetimeScope();
            return new AutofacWebApiDependencyScope(lifetimeScope);
        }

        public void Dispose()
        {
            autofacWebApiDependencyResolver.Dispose();
            autofacWebApiDependencyResolver = null;
            adapter = null;
        }
    }
}
