using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Supervisor.Activities;

namespace WB.UI.Supervisor
{
    public class SupervisorAppStart : MvxAppStart
    {
        public SupervisorAppStart(IMvxApplication application, IMvxNavigationService navigationService) : base(application, navigationService)
        {
        }

        protected override void Startup(object hint = null)
        {
            Mvx.Resolve<InterviewDashboardEventHandler>();

            var logger = Mvx.Resolve<ILoggerProvider>().GetFor<SupervisorAppStart>();
            logger.Warn($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            base.Startup(hint);
        }

        protected override void NavigateToFirstViewModel(object hint = null)
        {
            var viewModelNavigationService = Mvx.Resolve<IViewModelNavigationService>();

            var currentUser = Mvx.Resolve<IPlainStorage<SupervisorIdentity>>().FirstOrDefault();
            if (currentUser == null)
            {
                viewModelNavigationService.NavigateToFinishInstallationAsync().ConfigureAwait(false);
            }
            else
            {
                viewModelNavigationService.NavigateToLoginAsync().ConfigureAwait(false);
            }
        }
    }
}
