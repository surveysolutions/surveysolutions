using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Supervisor.Activities;

namespace WB.UI.Supervisor
{
    public class SupervisorAppStart : MvxAppStart
    {
        private readonly IViewModelNavigationService viewModelNavigation;
        private readonly IPlainStorage<SupervisorIdentity> users;

        public SupervisorAppStart(IMvxApplication application, IMvxNavigationService navigationService,
            IViewModelNavigationService viewModelNavigation,
            IPlainStorage<SupervisorIdentity> users
        ) : base(application, navigationService)
        {
            this.viewModelNavigation = viewModelNavigation;
            this.users = users;
        }

        protected override void Startup(object hint = null)
        {
            Mvx.Resolve<InterviewDashboardEventHandler>();

            var logger = Mvx.Resolve<ILoggerProvider>().GetFor<SupervisorAppStart>();
            logger.Info($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            base.Startup(hint);
        }

        protected override void NavigateToFirstViewModel(object hint = null)
        {
            var currentUser = users.FirstOrDefault();
            if (currentUser == null)
            {
                viewModelNavigation.NavigateToFinishInstallationAsync().WaitAndUnwrapException();
            }
            else
            {
                viewModelNavigation.NavigateToLoginAsync().WaitAndUnwrapException();
            }
        }
    }
}
