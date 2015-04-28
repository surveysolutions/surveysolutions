using Microsoft.Practices.ServiceLocation;
using Ninject.Modules;
using NinjectAdapter;

namespace WB.Core.Infrastructure.Android
{
    public class ServiceLocationModule : NinjectModule
    {
        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.Kernel));

            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);
        }
    }
}