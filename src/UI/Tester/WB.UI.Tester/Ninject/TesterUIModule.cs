using Ninject.Modules;

using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.CustomServices.UserInteraction;

namespace WB.UI.Tester.Ninject
{
    public class TesterUIModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IUserInteractionService>().To<UserInteractionService>();
            this.Bind<IUserInterfaceStateService>().To<UserInterfaceStateService>();
            this.Bind<IViewModelNavigationService>().To<ViewModelNavigationService>();
        }
    }
}