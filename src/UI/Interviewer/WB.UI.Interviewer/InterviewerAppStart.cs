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
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Interviewer.Activities;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Interviewer
{
    public class InterviewerAppStart : MvxAppStart
    {
        private readonly ILogger logger;
        private readonly IAuditLogService auditLogService;
        private readonly IServiceLocator serviceLocator;
        private readonly IApplicationCypher applicationCypher;

        public InterviewerAppStart(IMvxApplication application, 
            IMvxNavigationService navigationService,
            IAuditLogService auditLogService,
            IServiceLocator serviceLocator,
            IApplicationCypher applicationCypher,
            ILogger logger) : base(application, navigationService)
        {
            this.auditLogService = auditLogService;
            this.serviceLocator = serviceLocator;
            this.applicationCypher = applicationCypher;
            this.logger = logger;
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

            applicationCypher.EncryptAppData();

            this.BackwardCompatibility();

            this.CheckAndProcessAudit();
           
            return base.ApplicationStartup(hint);
        }

        protected override async Task NavigateToFirstViewModel(object hint = null)
        {
            var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
            var interviewersPlainStorage = Mvx.IoCProvider.Resolve<IPlainStorage<InterviewerIdentity>>();
            InterviewerIdentity currentInterviewer = interviewersPlainStorage.FirstOrDefault();
            logger.Debug("NavigateToFirstViewModel called");
            if (currentInterviewer == null)
            {
                await viewModelNavigationService.NavigateToFinishInstallationAsync();
            }
            else
            {
                await viewModelNavigationService.NavigateToLoginAsync();
            }
            logger.Debug("NavigateToFirstViewModel ended");
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
