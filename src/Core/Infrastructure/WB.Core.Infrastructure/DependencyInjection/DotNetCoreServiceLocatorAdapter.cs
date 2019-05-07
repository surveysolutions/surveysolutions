using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.DependencyInjection
{
    public class DotNetCoreServiceLocatorAdapter : ServiceLocatorImplBase
    {
        protected readonly IServiceProvider serviceProvider;

        public DotNetCoreServiceLocatorAdapter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override void InjectProperties(object instance)
        {
            throw new NotSupportedException();
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (key != null)
                throw new NotSupportedException(nameof(key));

            return serviceProvider.GetService(serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);

            object instance = serviceProvider.GetService(enumerableType);
            return ((IEnumerable) instance).Cast<object>();
        }
    }
}
