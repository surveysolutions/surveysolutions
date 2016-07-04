using System.Threading.Tasks;
using Android.Content;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Interviewer.Activities;
using WB.UI.Interviewer.ViewModel;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class ViewModelNavigationService : BaseViewModelNavigationService, IViewModelNavigationService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;

        public ViewModelNavigationService(ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity androidCurrentTopActivity,
            IPrincipal principal)
            : base(commandService, userInteractionService, userInterfaceStateService, principal)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
        }

        public void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel => this.NavigateTo<TViewModel>(null);

        public void NavigateToDashboard() => this.NavigateTo<DashboardViewModel>();
        public void NavigateToInterview(string interviewId) => this.NavigateTo<InterviewerInterviewViewModel>(new { interviewId = interviewId });
        public void NavigateToPrefilledQuestions(string interviewId) => this.NavigateTo<PrefilledQuestionsViewModel>(new { interviewId = interviewId });

        public override void NavigateToLogin() => this.NavigateTo<LoginViewModel>();
        protected override void DisposeViewModel() => this.androidCurrentTopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            this.androidCurrentTopActivity.Activity.StartActivity(new Intent(this.androidCurrentTopActivity.Activity, typeof(PrefsActivity)));
    }
}