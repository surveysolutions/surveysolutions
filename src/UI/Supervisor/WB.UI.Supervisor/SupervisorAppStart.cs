using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Supervisor.Activities;

namespace WB.UI.Supervisor
{
    public class SupervisorAppStart : MvxAppStart
    {
        private readonly IViewModelNavigationService viewModelNavigation;
        private readonly IPlainStorage<SupervisorIdentity> users;
        private readonly IApplicationCypher applicationCypher;

        public SupervisorAppStart(IMvxApplication application, IMvxNavigationService navigationService,
            IViewModelNavigationService viewModelNavigation,
            IPlainStorage<SupervisorIdentity> users,
            IApplicationCypher applicationCypher
        ) : base(application, navigationService)
        {
            this.viewModelNavigation = viewModelNavigation;
            this.users = users;
            this.applicationCypher = applicationCypher;
        }

        protected override Task<object> ApplicationStartup(object hint = null)
        {
            Mvx.IoCProvider.GetSingleton<InterviewDashboardEventHandler>();

            var logger = Mvx.IoCProvider.Resolve<ILoggerProvider>().GetFor<SupervisorAppStart>();
            logger.Info($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            applicationCypher.EncryptAppData();

            return base.ApplicationStartup(hint);
        }

        protected override Task NavigateToFirstViewModel(object hint = null)
        {
            var currentUser = users.FirstOrDefault();
            if (currentUser == null)
            {
                return viewModelNavigation.NavigateToFinishInstallationAsync();
            }
            else
            {
                return viewModelNavigation.NavigateToLoginAsync();
            }
        }
    }
}
