using System;
using System.Threading.Tasks;
using Android.Content;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Interviewer.Activities;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class ViewModelNavigationService : BaseViewModelNavigationService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IMvxNavigationService navigationService;

        public ViewModelNavigationService(
            ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity androidCurrentTopActivity,
            IPrincipal principal,
            IMvxNavigationService navigationService)
            : base(commandService, userInteractionService, userInterfaceStateService, androidCurrentTopActivity, navigationService, principal)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.navigationService = navigationService;
        }

        public override Task NavigateToDashboardAsync(string interviewId = null)
        {
            if (interviewId == null)
            {
                return this.navigationService.Navigate<DashboardViewModel>();
            }

            return this.navigationService.Navigate<DashboardViewModel, DashboardViewModelArgs>(new DashboardViewModelArgs
            {
                InterviewId = Guid.Parse(interviewId)
            });
        }

        public override Task NavigateToPrefilledQuestionsAsync(string interviewId) => 
            this.navigationService.Navigate<PrefilledQuestionsViewModel, InterviewViewModelArgs>(new InterviewViewModelArgs
            {
                InterviewId = interviewId
            });

        public override void NavigateToSplashScreen()
        {
            base.RestartApp(typeof(SplashActivity));
        }

        public override Task NavigateToFinishInstallationAsync()
            => this.NavigateToAsync<FinishInstallationViewModel>();

        public override Task NavigateToMapsAsync() => this.NavigateToAsync<MapsViewModel>();

        public override Task NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity)
            => this.navigationService.Navigate<InterviewViewModel, InterviewViewModelArgs>(new InterviewViewModelArgs
            {
                InterviewId = interviewId,
                NavigationIdentity = navigationIdentity
            });

        public override Task NavigateToLoginAsync() => this.NavigateToAsync<LoginViewModel>();
        protected override void FinishActivity() => this.androidCurrentTopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            this.androidCurrentTopActivity.Activity.StartActivity(new Intent(this.androidCurrentTopActivity.Activity, typeof(PrefsActivity)));
    }
}
