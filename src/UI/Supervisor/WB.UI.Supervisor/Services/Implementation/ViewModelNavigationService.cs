using System;
using System.Threading.Tasks;
using Android.Content;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Supervisor.Activities;

namespace WB.UI.Supervisor.Services.Implementation
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

        public override Task NavigateToPrefilledQuestionsAsync(string interviewId) => 
            this.navigationService.Navigate<PrefilledQuestionsViewModel, InterviewViewModelArgs>(new InterviewViewModelArgs
            {
                InterviewId = interviewId
            });

        public override void NavigateToSplashScreen() => base.RestartApp(typeof(SplashActivity));

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

        public override Task NavigateToLoginAsync() => this.NavigateToAsync<LoginViewModel>();
        public override Task NavigateToFinishInstallationAsync() => this.NavigateToAsync<FinishInstallationViewModel>();

        public override Task NavigateToMapsAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity)
            => this.navigationService.Navigate<SupervisorInterviewViewModel, InterviewViewModelArgs>(new InterviewViewModelArgs
            {
                InterviewId = interviewId,
                NavigationIdentity = navigationIdentity
            });

        protected override void FinishActivity() => this.androidCurrentTopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            this.androidCurrentTopActivity.Activity.StartActivity(new Intent(this.androidCurrentTopActivity.Activity, typeof(PrefsActivity)));
    }
}
