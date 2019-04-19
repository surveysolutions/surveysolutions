using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Interviewer.Activities;
using WB.UI.Shared.Enumerator.Migrations;

namespace WB.UI.Interviewer
{
    public class InterviewerAppStart : MvxAppStart
    {
        private readonly ILogger logger;
        private readonly IMigrationRunner migrationRunner;
        private readonly IAuditLogService auditLogService;
        private readonly IServiceLocator serviceLocator;

        public InterviewerAppStart(IMvxApplication application, 
            IMvxNavigationService navigationService,
            IAuditLogService auditLogService,
            IServiceLocator serviceLocator,
            ILogger logger,
            IMigrationRunner migrationRunner) : base(application, navigationService)
        {
            this.auditLogService = auditLogService;
            this.serviceLocator = serviceLocator;
            this.logger = logger;
            this.migrationRunner = migrationRunner;
        }

        public override void ResetStart()
        {
            //temp fix of KP-11583
            //
            //base.ResetStart();
            logger.Warn("Ignored application reset start");
        }

        protected override Task<object> ApplicationStartup(object hint = null)
        {
            auditLogService.Write(new OpenApplicationAuditLogEntity());
            this.serviceLocator.GetInstance<InterviewDashboardEventHandler>();

            logger.Info($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            migrationRunner.MigrateUp(this.GetType().Assembly, typeof(Encrypt_Data).Assembly);

            Mvx.IoCProvider.Resolve<IAudioAuditService>().CheckAndProcessAllAuditFiles();

            return base.ApplicationStartup(hint);
        }

        protected override async Task NavigateToFirstViewModel(object hint = null)
        {
            var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
            var interviewersPlainStorage = Mvx.IoCProvider.Resolve<IPlainStorage<InterviewerIdentity>>();
            InterviewerIdentity currentInterviewer = interviewersPlainStorage.FirstOrDefault();
            if (currentInterviewer == null)
            {
                await viewModelNavigationService.NavigateToFinishInstallationAsync();
            }
            else
            {
                await viewModelNavigationService.NavigateToLoginAsync();
            }
        }
    }
}
