using Ninject.Modules;

using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Tester.Implementation.Services;

namespace WB.UI.Tester.Ninject
{
    public class TesterUIModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewModelNavigationService>().To<ViewModelNavigationService>();
        }
    }
}