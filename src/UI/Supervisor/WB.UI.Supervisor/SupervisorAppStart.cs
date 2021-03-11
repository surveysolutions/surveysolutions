using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Shared.Enumerator.Migrations;
using WB.UI.Shared.Enumerator.Migrations.Workspaces;
using WB.UI.Supervisor.Activities;

namespace WB.UI.Supervisor
{
    public class SupervisorAppStart : MvxAppStart
    {
        private readonly IViewModelNavigationService viewModelNavigation;
        private readonly IPlainStorage<SupervisorIdentity> users;
        private readonly IMigrationRunner migrationRunner;

        public SupervisorAppStart(IMvxApplication application, IMvxNavigationService navigationService,
            IViewModelNavigationService viewModelNavigation,
            IPlainStorage<SupervisorIdentity> users,
            IMigrationRunner migrationRunner
        ) : base(application, navigationService)
        {
            this.viewModelNavigation = viewModelNavigation;
            this.users = users;
            this.migrationRunner = migrationRunner;
        }

        protected override Task<object> ApplicationStartup(object hint = null)
        {
            var logger = Mvx.IoCProvider.Resolve<ILoggerProvider>().GetFor<SupervisorAppStart>();
            logger.Info($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            this.migrationRunner.MigrateUp(this.GetType().Assembly, typeof(Encrypt_Data).Assembly);

            this.CheckAndProcessInterviews();
            return base.ApplicationStartup(hint);
        }

        private void CheckAndProcessInterviews()
        {
            var settings = Mvx.IoCProvider.Resolve<IEnumeratorSettings>();
            if (settings.DashboardViewsUpdated) return;

            var interviewsAccessor = Mvx.IoCProvider.Resolve<IInterviewerInterviewAccessor>();
            interviewsAccessor.CheckAndProcessInterviewsToFixViews();

            settings.SetDashboardViewsUpdated(true);
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
