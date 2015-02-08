using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Ninject;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NinjectServiceLocator : ServiceLocatorImplBase
    {
        public IKernel Kernel { get; private set; }

        public NinjectServiceLocator(IKernel kernel)
        {
            this.Kernel = kernel;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            return this.Kernel.Get(serviceType, key);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return this.Kernel.GetAll(serviceType);
        }
    }
}