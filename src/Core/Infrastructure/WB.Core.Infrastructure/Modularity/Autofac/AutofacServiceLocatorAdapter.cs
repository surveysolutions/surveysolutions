using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class AutofacServiceLocatorAdapter : ServiceLocatorImplBase
    {
        private readonly ILifetimeScope container;

        public AutofacServiceLocatorAdapter(ILifetimeScope kernel)
        {
            this.container = kernel;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            return key != null ? container.ResolveNamed(key, serviceType) : container.Resolve(serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            object instance = container.Resolve(enumerableType);
            return ((IEnumerable) instance).Cast<object>();
        }

        public ILifetimeScope CreateChildContainer(Action<ContainerBuilder> @override)
        {
            return this.container.BeginLifetimeScope(@override);
        }

        public ILifetimeScope CreateChildContainer()
        {
            return this.container.BeginLifetimeScope();
        }
    }

}
