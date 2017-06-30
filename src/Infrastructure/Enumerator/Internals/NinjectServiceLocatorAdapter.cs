using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using Ninject.Parameters;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    public class NinjectServiceLocatorAdapter : ServiceLocatorImplBase
    {
        private readonly IKernel kernel;

        public NinjectServiceLocatorAdapter(IKernel kernel)
        {
            this.kernel = kernel;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (!string.IsNullOrEmpty(key))
                return this.kernel.Get(serviceType, key, new IParameter[0]);
            return this.kernel.Get(serviceType, (string)null, new IParameter[0]);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return this.kernel.GetAll(serviceType, new IParameter[0]);
        }
    }
}