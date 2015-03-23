using Microsoft.Practices.ServiceLocation;
using Ninject.Modules;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class ServiceLocationModule : NinjectModule
    {
        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.Kernel));
            this.Kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);
        }
    }
}
