using Cirrious.MvvmCross.Plugins.Location;
using Ninject.Modules;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.UI.Shared.Enumerator.Internals;

namespace WB.UI.Shared.Enumerator.Ninject
{
    public class EnumeratorUIModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewModelNavigationService>().To<ViewModelNavigationService>();
        }
    }
}