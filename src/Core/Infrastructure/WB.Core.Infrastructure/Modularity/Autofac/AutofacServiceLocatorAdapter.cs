using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Modularity.Autofac
{
    public class AutofacServiceLocatorAdapter : ServiceLocatorImplBase
    {
        protected readonly ILifetimeScope rootScope;

        public AutofacServiceLocatorAdapter(ILifetimeScope kernel)
        {
            this.rootScope = kernel;
        }

        public override void InjectProperties(object instance)
        {
            this.rootScope.InjectProperties(instance);
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            return key != null ? rootScope.ResolveNamed(key, serviceType) : rootScope.Resolve(serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            object instance = rootScope.Resolve(enumerableType);
            return ((IEnumerable) instance).Cast<object>();
        }
    }

}
