using System;
using Microsoft.Practices.ServiceLocation;
using Ninject.Modules;
using NinjectAdapter;

namespace Web.Supervisor
{
    internal class ServiceLocationModule : NinjectModule
    {
        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.Kernel));

            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);
        }
    }
}