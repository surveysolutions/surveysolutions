using System.Linq;
using System.Threading.Tasks;
using Autofac;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Migrations.Workspaces;
using WB.UI.Shared.Enumerator.Services.Notifications;

namespace WB.UI.Interviewer
{
    public class InterviewerAppStart : MvxAppStart
    {
        private readonly ILogger logger;
        private readonly IMigrationRunner migrationRunner;
        private readonly ILifetimeScope lifetimeScope;
        private readonly IWorkspaceService workspaceService;
        private readonly IAuditLogService auditLogService;
        private readonly IEnumeratorSettings enumeratorSettings;
        private readonly IDeviceSettings deviceSettings;
        
        public InterviewerAppStart(IMvxApplication application, 
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings, 
            ILogger logger,
            IMigrationRunner migrationRunner,
            ILifetimeScope lifetimeScope,
            IWorkspaceService workspaceService,
            IDeviceSettings deviceSettings) : base(application, Mvx.IoCProvider.Resolve<IMvxNavigationService>())
        {
            this.auditLogService = auditLogService;
            this.logger = logger;
            this.migrationRunner = migrationRunner;
            this.lifetimeScope = lifetimeScope;
            this.workspaceService = workspaceService;
            this.enumeratorSettings = enumeratorSettings;
            this.deviceSettings = deviceSettings;
        }

        protected override Task<object> ApplicationStartup(object hint = null)
        {
            auditLogService.WriteApplicationLevelRecord(new OpenApplicationAuditLogEntity());

            logger.Info($"Application started. Version: {this.deviceSettings.GetApplicationVersionName()}");

            migrationRunner.MigrateUp("Interviewer", this.GetType().Assembly, typeof(Encrypt_Data).Assembly);

            CheckAndProcessAllAuditFiles();
            
            this.UpdateNotificationsWorker();

            this.CheckAndProcessUserLogins();

            return base.ApplicationStartup(hint);
        }

        private void CheckAndProcessAllAuditFiles()
        {
            var workspaces = workspaceService.GetAll();
            foreach (var workspace in workspaces)
            {
                var workspaceAccessor = new SingleWorkspaceAccessor(workspace.Name);
                using var workspaceLifetimeScope = lifetimeScope.BeginLifetimeScope(cb =>
                {
                    cb.Register(c => workspaceAccessor).As<IWorkspaceAccessor>().SingleInstance();
                    cb.RegisterType<AudioService>().As<IAudioService>()
                        .WithParameter("audioDirectory", "audio");
                });
                workspaceLifetimeScope.Resolve<IAudioAuditService>().CheckAndProcessAllAuditFiles();
            }
        }

        private void CheckAndProcessUserLogins()
        {
            var interviewersStorage = Mvx.IoCProvider.Resolve<IPlainStorage<InterviewerIdentity>>();
            
            var users = interviewersStorage.LoadAll().ToList();
            if (users.Count > 1)
            {
                var interviewViewRepository = Mvx.IoCProvider.Resolve<IPlainStorage<InterviewView>>();
                var assignmentViewRepository = Mvx.IoCProvider.Resolve<IAssignmentDocumentsStorage>();

                foreach (var interviewerIdentity in users)
                {
                    var interviewsCount =
                        interviewViewRepository.Count(x => x.ResponsibleId == interviewerIdentity.UserId);

                    if(interviewsCount > 0)
                        continue;
                    var assignmentsCount =
                        assignmentViewRepository.Count(x => x.ResponsibleId == interviewerIdentity.UserId);

                    if (assignmentsCount == 0)
                    {
                        logger.Warn($"Removing extra user {interviewerIdentity.Name}, Id: {interviewerIdentity.Id}");
                        interviewersStorage.Remove(interviewerIdentity.Id);
                    }
                }
            }
        }

        private void UpdateNotificationsWorker()
        {
            var workerManager = Mvx.IoCProvider.Resolve<IEnumeratorWorkerManager>();

            if(enumeratorSettings.NotificationsEnabled)
                workerManager.SetNotificationsWorker();
            else
                workerManager.CancelNotificationsWorker();
        }

        protected override async Task NavigateToFirstViewModel(object hint = null)
        {
            var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
            var interviewerPrincipal = Mvx.IoCProvider.Resolve<IInterviewerPrincipal>();
            
            if (!interviewerPrincipal.DoesIdentityExist())
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
