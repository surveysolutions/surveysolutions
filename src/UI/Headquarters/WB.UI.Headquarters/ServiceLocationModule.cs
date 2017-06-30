using Microsoft.Practices.ServiceLocation;
using Ninject.Modules;
using WB.Infrastructure.Native.Ioc;

namespace WB.UI.Headquarters
{
    internal class ServiceLocationModule : NinjectModule
    {
        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NativeNinjectServiceLocatorAdapter(this.Kernel));

            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);
        }
    }
}