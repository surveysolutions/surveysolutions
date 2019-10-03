using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Interviewer.Activities;
using WB.UI.Shared.Enumerator.Migrations;
using WB.UI.Shared.Enumerator.Services.Notifications;

namespace WB.UI.Interviewer
{
    public class InterviewerAppStart : MvxAppStart
    {
        private readonly ILogger logger;
        private readonly IMigrationRunner migrationRunner;
        private readonly IAuditLogService auditLogService;
        private readonly IServiceLocator serviceLocator;
        private readonly IEnumeratorSettings enumeratorSettings;
        
        public InterviewerAppStart(IMvxApplication application, 
            IMvxNavigationService navigationService,
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings, 
            IServiceLocator serviceLocator,
            ILogger logger,
            IMigrationRunner migrationRunner) : base(application, navigationService)
        {
            this.auditLogService = auditLogService;
            this.serviceLocator = serviceLocator;
            this.logger = logger;
            this.migrationRunner = migrationRunner;
            this.enumeratorSettings = enumeratorSettings;
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

            this.serviceLocator.GetInstance<IDenormalizerRegistry>()
                .RegisterDenormalizer(this.serviceLocator.GetInstance<InterviewDashboardEventHandler>());

            logger.Info($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            migrationRunner.MigrateUp(this.GetType().Assembly, typeof(Encrypt_Data).Assembly);

            Mvx.IoCProvider.Resolve<IAudioAuditService>().CheckAndProcessAllAuditFiles();
            
            this.UpdateNotificationsWorker();

            this.CheckAndProcessInterviewsWithoutViews();

            this.CheckAndProcessUserLogins();

            return base.ApplicationStartup(hint);
        }

        private void CheckAndProcessUserLogins()
        {
            //fix of KP-13229 

            var interviewersStorage = Mvx.IoCProvider.Resolve<IPlainStorage<InterviewerIdentity>>();
            
            var users = interviewersStorage.Where(x => x.IsDeleted == null || x.IsDeleted == false).ToList();
            if (users.Count > 1)
            {
                var assignmentViewRepository = Mvx.IoCProvider.Resolve<IAssignmentDocumentsStorage>();
                var auditLog = Mvx.IoCProvider.Resolve<IAuditLogService>();
                
                //getting first user from audit log
                Guid firstUserId = Guid.Empty;
                foreach (var auditLogEntityView in auditLog.GetAllAuditLogEntities())
                {
                    if (auditLogEntityView.ResponsibleId != null)
                    {
                        firstUserId = auditLogEntityView.ResponsibleId.Value;
                        break;
                    }
                }

                if (firstUserId == Guid.Empty)
                {
                    var assignmentResponsibles = assignmentViewRepository.LoadAll().Select(x => x.ResponsibleId)
                        .Distinct().ToList();
                    //assuming only original user pulled assignments
                    if (assignmentResponsibles.Count == 1)
                    {
                        firstUserId = assignmentResponsibles.First();
                    }
                }

                if (firstUserId != Guid.Empty)
                {
                    foreach (var interviewerIdentity in users)
                    {
                        if (interviewerIdentity.UserId != firstUserId)
                        {
                            logger.Warn(
                                $"Marking as deleted extra user {interviewerIdentity.Name}, Id: {interviewerIdentity.Id}");

                            interviewerIdentity.IsDeleted = true;
                            interviewersStorage.Store(interviewerIdentity);
                        }
                    }
                }
            }
        }

        private void CheckAndProcessInterviewsWithoutViews()
        {
            var interviewsAccessor = Mvx.IoCProvider.Resolve<IInterviewerInterviewAccessor>();
            interviewsAccessor.CheckAndProcessInterviewsWithoutViews();
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
