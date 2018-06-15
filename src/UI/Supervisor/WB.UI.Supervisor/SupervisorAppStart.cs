using MvvmCross;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Supervisor.Activities;

namespace WB.UI.Supervisor
{
    public class SupervisorAppStart : MvxAppStart
    {
        public SupervisorAppStart(IMvxApplication application) : base(application)
        {
        }

        protected override async void Startup(object hint = null)
        {
            var logger = Mvx.Resolve<ILoggerProvider>().GetFor<SupervisorAppStart>();
            logger.Warn($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            base.ApplicationStartup(hint);

            var viewModelNavigationService = Mvx.Resolve<IViewModelNavigationService>();

            var currentUser = Mvx.Resolve<IPlainStorage<SupervisorIdentity>>().FirstOrDefault();
            if (currentUser == null)
            {
                await viewModelNavigationService.NavigateToFinishInstallationAsync().ConfigureAwait(false);
            }
            else
            {
                await viewModelNavigationService.NavigateToLoginAsync().ConfigureAwait(false);
            }
        }
    }
}
