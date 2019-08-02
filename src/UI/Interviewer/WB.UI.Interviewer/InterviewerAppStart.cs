using System.Diagnostics;
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
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Interviewer.Activities;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Notifications;

namespace WB.UI.Interviewer
{
    public class InterviewerAppStart : MvxAppStart
    {
        private readonly ILogger logger;
        private readonly IAuditLogService auditLogService;
        private readonly IServiceLocator serviceLocator;
        private readonly IApplicationCypher applicationCypher;
        private IEnumeratorSettings enumeratorSettings;

        //preserve link and saving object to avoid collection with GC
        private static InterviewDashboardEventHandler interviewDashboardEventHandler = null;

        public InterviewerAppStart(IMvxApplication application, 
            IMvxNavigationService navigationService,
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings, 
            IServiceLocator serviceLocator,
            IApplicationCypher applicationCypher,
            ILogger logger) : base(application, navigationService)
        {
            this.auditLogService = auditLogService;
            this.serviceLocator = serviceLocator;
            this.applicationCypher = applicationCypher;
            this.logger = logger;
            this.enumeratorSettings = enumeratorSettings;
        }

        public override void ResetStart()
        {
            //temp fix of KP-11583
            //
            //base.ResetStart();
            if (interviewDashboardEventHandler == null)
            {
                logger.Warn("Instance if InterviewDashboardEventHandler was lost!");
                interviewDashboardEventHandler = this.serviceLocator.GetInstance<InterviewDashboardEventHandler>();
            }

            logger.Warn("Ignored application reset start");
        }

        protected override Task<object> ApplicationStartup(object hint = null)
        {
            auditLogService.Write(new OpenApplicationAuditLogEntity());
            interviewDashboardEventHandler = this.serviceLocator.GetInstance<InterviewDashboardEventHandler>();

            logger.Info($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            applicationCypher.EncryptAppData();

            this.BackwardCompatibility();

            this.CheckAndProcessAudit();

            this.UpdateNotificationsWorker();

            this.CheckAndProcessInterviewsWithoutViews();

            return base.ApplicationStartup(hint);
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


        [Conditional("RELEASE")]
        private void BackwardCompatibility()
        {
            this.UpdateAssignmentsWithInterviewsCount();
            this.AddTitleToOptionViewForSearching();
        }

        private void UpdateAssignmentsWithInterviewsCount()
        {
            var assignmentStorage = Mvx.IoCProvider.Resolve<IAssignmentDocumentsStorage>();

            var hasEmptyInterviewsCounts = assignmentStorage.Count(x => x.CreatedInterviewsCount == null) > 0;
            
            if (!hasEmptyInterviewsCounts) return;

            var interviewStorage = Mvx.IoCProvider.Resolve<IPlainStorage<InterviewView>>();
            
            var assignments = assignmentStorage.LoadAll();

            foreach (var assignment in assignments)
            {
                assignment.CreatedInterviewsCount = interviewStorage.Count(x => x.CanBeDeleted && x.Assignment == assignment.Id);
                assignmentStorage.Store(assignment);
            }
        }

        private void AddTitleToOptionViewForSearching()
        {
            var optionsStorage = Mvx.IoCProvider.Resolve<IPlainStorage<OptionView>>();

            var hasEmptySearchTitles = optionsStorage.Count(x => x.SearchTitle == null) > 0;
            if (!hasEmptySearchTitles) return;

            var allOptions = optionsStorage.LoadAll();

            foreach (var optionView in allOptions)
                optionView.SearchTitle = optionView.Title.ToLower();
            
            optionsStorage.Store(allOptions);

        }

        private void CheckAndProcessAudit()
        {
            var auditService = Mvx.IoCProvider.Resolve<IAudioAuditService>();
            auditService.CheckAndProcessAllAuditFiles();
        }
    }
}
