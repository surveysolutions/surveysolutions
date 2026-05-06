using System.Threading.Tasks;
using Autofac;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.UI.Shared.Enumerator.Migrations.Workspaces;

namespace WB.UI.Supervisor
{
    public class SupervisorAppStart : MvxAppStart
    {
        private readonly IViewModelNavigationService viewModelNavigation;
        private readonly IPlainStorage<SupervisorIdentity> users;
        private readonly IMigrationRunner migrationRunner;
        private readonly IWorkspaceService workspaceService;
        private readonly ILifetimeScope lifetimeScope;
        private readonly ISupervisorSettings deviceSettings;
        private bool downgradeDetected = false;

        public SupervisorAppStart(IMvxApplication application,
            IViewModelNavigationService viewModelNavigation,
            IPlainStorage<SupervisorIdentity> users,
            IMigrationRunner migrationRunner,
            IWorkspaceService workspaceService,
            ILifetimeScope lifetimeScope,
            ISupervisorSettings deviceSettings
        ) : base(application, Mvx.IoCProvider.Resolve<IMvxNavigationService>())
        {
            this.viewModelNavigation = viewModelNavigation;
            this.users = users;
            this.migrationRunner = migrationRunner;
            this.workspaceService = workspaceService;
            this.lifetimeScope = lifetimeScope;
            this.deviceSettings = deviceSettings;
        }

        protected override Task<object> ApplicationStartup(object hint = null)
        {
            var logger = Mvx.IoCProvider.Resolve<ILoggerProvider>().GetFor<SupervisorAppStart>();
            logger.Info($"Application started. Version: {this.deviceSettings.GetApplicationVersionName()}");
            logger.Info($"Android Version: {this.deviceSettings.GetAndroidVersion()}");
            logger.Info($"Google Play Services Version: {this.deviceSettings.GetGooglePlayServicesVersion()}");
            logger.Info($"Disk: {this.deviceSettings.GetDiskInformation()}");

            var currentVersionCode = this.deviceSettings.GetApplicationVersionCode();
            var lastKnownVersionCode = this.deviceSettings.GetLastKnownAppVersionCode();

            if (lastKnownVersionCode.HasValue && currentVersionCode < lastKnownVersionCode.Value)
            {
                logger.Error($"Downgrade detected. Current version: {currentVersionCode}, last known version: {lastKnownVersionCode.Value}");
                downgradeDetected = true;
                return base.ApplicationStartup(hint);
            }

            this.deviceSettings.SetLastKnownAppVersionCode(currentVersionCode);

            this.migrationRunner.MigrateUp("Supervisor", this.GetType().Assembly, typeof(Encrypt_Data).Assembly);

            return base.ApplicationStartup(hint);
        }

        protected override async Task NavigateToFirstViewModel(object hint = null)
        {
            if (downgradeDetected)
            {
                var userInteractionService = Mvx.IoCProvider.Resolve<IUserInteractionService>();
                await userInteractionService.AlertAsync(EnumeratorUIResources.Downgrade_ErrorMessage);
                viewModelNavigation.CloseApplication();
                return;
            }

            var currentUser = users.FirstOrDefault();
            if (currentUser == null)
            {
                await viewModelNavigation.NavigateToFinishInstallationAsync();
            }
            else
            {
                await viewModelNavigation.NavigateToLoginAsync();
            }
        }
    }
}
