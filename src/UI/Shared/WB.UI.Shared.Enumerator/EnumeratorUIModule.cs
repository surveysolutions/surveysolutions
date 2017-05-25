using Ninject.Modules;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Infrastructure.Shared.Enumerator.Internals.MapService;
using WB.UI.Shared.Enumerator.CustomServices;

namespace WB.UI.Shared.Enumerator
{
    public class EnumeratorUIModule : NinjectModule
    {
        public override void Load()
        {
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());

            this.Bind<IUserInteractionService>().To<UserInteractionService>();
            this.Bind<IUserInterfaceStateService>().To<UserInterfaceStateService>();

            this.Bind<IExternalAppLauncher>().To<ExternalAppLauncher>();
            this.Bind<IVirbationService>().To<VibrationService>();

            this.Bind<IMapService>().To<MapService>();
        }
    }
}