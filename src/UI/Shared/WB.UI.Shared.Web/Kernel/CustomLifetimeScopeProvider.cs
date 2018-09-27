using System;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using WB.Core.Infrastructure.Modularity.Autofac;

namespace WB.UI.Shared.Web.Kernel
{
    public class CustomLifetimeScopeProvider : ILifetimeScopeProvider
    {
        private readonly AutofacServiceLocatorAdapterWithChildrenScopes scopes;

        public CustomLifetimeScopeProvider(AutofacServiceLocatorAdapterWithChildrenScopes scopes)
        {
            this.scopes = scopes;
        }

        public ILifetimeScope GetLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return scopes.GetCurrentScope();
        }

        public void EndLifetimeScope()
        {
            
        }

        public ILifetimeScope ApplicationContainer => scopes.GetRootScope();
    }
}
