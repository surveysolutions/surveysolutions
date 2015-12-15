using Ninject.Modules;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.CustomServices;

namespace WB.UI.Shared.Enumerator
{
    public class EnumeratorUIModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IUserInteractionService>().To<UserInteractionService>();
            this.Bind<IUserInterfaceStateService>().To<UserInterfaceStateService>();

            this.Bind<IExternalAppLauncher>().To<ExternalAppLauncher>();
            this.Bind<ITabletDiagnosticService>().To<TabletDiagnosticService>();
        }
    }
}