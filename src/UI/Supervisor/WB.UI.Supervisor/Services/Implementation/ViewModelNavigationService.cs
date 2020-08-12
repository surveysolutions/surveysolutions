using System;
using System.Threading.Tasks;
using Android.Content;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ILogger log;

        public ViewModelNavigationService(
            ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity androidCurrentTopActivity,
            IPrincipal principal,
            IMvxNavigationService navigationService,
            ILogger logger)
            : base(commandService: commandService,
                userInteractionService,
                userInterfaceStateService,
                androidCurrentTopActivity,
                navigationService,
                principal,
                logger)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.navigationService = navigationService;
            this.log = logger;
        }

        public override Task NavigateToPrefilledQuestionsAsync(string interviewId)
        {
            this.log.Trace($"Navigating to PrefilledQuestionsViewModel interviewId: {interviewId}");
            return this.navigationService.Navigate<SupervisorInterviewViewModel, InterviewViewModelArgs>(
                new InterviewViewModelArgs
                {
                    InterviewId = interviewId,
                    NavigationIdentity = NavigationIdentity.CreateForCoverScreen()
                });
        }

        public override void NavigateToSplashScreen() => base.RestartApp(typeof(SplashActivity));

        public override async Task<bool> NavigateToDashboardAsync(string interviewId = null)
        {
            this.log.Trace($"Navigating to dashboard interviewId: {interviewId ?? "'null'"}");
            if (interviewId == null)
            {
                return await this.navigationService.Navigate<DashboardViewModel>();
            }

            return await this.navigationService.Navigate<DashboardViewModel, DashboardViewModelArgs>(new DashboardViewModelArgs
            {
                InterviewId = Guid.Parse(interviewId)
            });
        }

        public override Task NavigateToCreateAndLoadInterview(int assignmentId)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        public override Task NavigateToLoginAsync()
        {
            this.log.Trace("Navigating to LoginViewModel");
            return this.NavigateToAsync<LoginViewModel>();
        }

        public override Task NavigateToFinishInstallationAsync()
        {
            this.log.Trace("Navigating to FinishInstallationViewModel");
            return this.NavigateToAsync<FinishInstallationViewModel>();
        }

        public override Task NavigateToMapsAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity)
        {
            this.log.Trace("Navigating to SupervisorInterviewViewModel");
            return this.navigationService.Navigate<SupervisorInterviewViewModel, InterviewViewModelArgs>(
                new InterviewViewModelArgs
                {
                    InterviewId = interviewId,
                    NavigationIdentity = navigationIdentity
                });
        }

        protected override void FinishActivity() => this.androidCurrentTopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            this.androidCurrentTopActivity.Activity.StartActivity(new Intent(this.androidCurrentTopActivity.Activity, typeof(PrefsActivity)));
    }
}
