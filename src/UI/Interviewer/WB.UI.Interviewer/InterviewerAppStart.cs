using System.Diagnostics;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
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

        protected override void Startup(object hint = null)
        {
            Mvx.Resolve<InterviewDashboardEventHandler>();

            var logger = Mvx.Resolve<ILoggerProvider>().GetFor<InterviewerAppStart>();
            logger.Warn($"Application started. Version: {typeof(SplashActivity).Assembly.GetName().Version}");

            this.BackwardCompatibility();
           
            base.Startup(hint);
        }

        protected override void NavigateToFirstViewModel(object hint = null)
        {
            var viewModelNavigationService = Mvx.Resolve<IViewModelNavigationService>();
            var interviewersPlainStorage = Mvx.Resolve<IPlainStorage<InterviewerIdentity>>();
            InterviewerIdentity currentInterviewer = interviewersPlainStorage.FirstOrDefault();

            if (currentInterviewer == null)
            {
                viewModelNavigationService.NavigateToFinishInstallationAsync().ConfigureAwait(false);
            }
            else
            {
                viewModelNavigationService.NavigateToLoginAsync().ConfigureAwait(false);
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
            var assignmentStorage = Mvx.Resolve<IAssignmentDocumentsStorage>();

            var hasEmptyInterviewsCounts = assignmentStorage.Count(x => x.CreatedInterviewsCount == null) > 0;
            
            if (!hasEmptyInterviewsCounts) return;

            var interviewStorage = Mvx.Resolve<IPlainStorage<InterviewView>>();
            
            var assignments = assignmentStorage.LoadAll();

            foreach (var assignment in assignments)
            {
                assignment.CreatedInterviewsCount = interviewStorage.Count(x => x.CanBeDeleted && x.Assignment == assignment.Id);
                assignmentStorage.Store(assignment);
            }
        }

        private void AddTitleToOptionViewForSearching()
        {
            var optionsStorage = Mvx.Resolve<IPlainStorage<OptionView>>();

            var hasEmptySearchTitles = optionsStorage.Count(x => x.SearchTitle == null) > 0;
            if (!hasEmptySearchTitles) return;

            var allOptions = optionsStorage.LoadAll();

            foreach (var optionView in allOptions)
                optionView.SearchTitle = optionView.Title.ToLower();
            
            optionsStorage.Store(allOptions);

        }
    }
}
