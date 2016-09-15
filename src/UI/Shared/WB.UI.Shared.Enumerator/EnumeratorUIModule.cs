using Ninject.Modules;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.UI.Shared.Enumerator.CustomServices;

namespace WB.UI.Shared.Enumerator
{
    public class EnumeratorUIModule : NinjectModule
    {
        public override void Load()
        {
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_esqlite3());

            this.Bind<IUserInteractionService>().To<UserInteractionService>();
            this.Bind<IUserInterfaceStateService>().To<UserInterfaceStateService>();

            this.Bind<IExternalAppLauncher>().To<ExternalAppLauncher>();
            this.Bind<SubstitutionViewModel>().ToSelf();
        }
    }
}