using System.Diagnostics;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Interviewer.Activities;

namespace WB.UI.Interviewer
{
    public class InterviewerAppStart : MvxAppStart
    {
        public InterviewerAppStart(IMvxApplication application, IMvxNavigationService navigationService) : base(application, navigationService)
        {
        }

        public override void ResetStart()
        {
            //temp fix ofKP-11583
            //
            //base.ResetStart();
        }

        protected override async Task<object> ApplicationStartup(object hint = null)
        {
            Mvx.IoCProvider.Resolve<InterviewDashboardEventHandler>();

            var logger = Mvx.IoCProvider.Resolve<ILoggerProvider>().GetFor<InterviewerAppStart>();
            logger.Warn($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            this.BackwardCompatibility();
           
            return await base.ApplicationStartup(hint);
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
    }
}
