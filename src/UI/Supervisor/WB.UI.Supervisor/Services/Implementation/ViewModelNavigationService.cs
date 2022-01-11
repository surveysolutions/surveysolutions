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

        private readonly ILogger log;

        public ViewModelNavigationService(
            ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IPrincipal principal,
            ILogger logger)
            : base(commandService: commandService,
                userInteractionService,
                userInterfaceStateService,
                principal,
                logger)
        {

            this.log = logger;
        }

        public override Task NavigateToPrefilledQuestionsAsync(string interviewId)
        {
            this.log.Trace($"Navigating to PrefilledQuestionsViewModel interviewId: {interviewId}");
            return NavigationService.Navigate<SupervisorInterviewViewModel, InterviewViewModelArgs>(
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
                return await NavigationService.Navigate<DashboardViewModel>();
            }

            return await NavigationService.Navigate<DashboardViewModel, DashboardViewModelArgs>(new DashboardViewModelArgs
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
            return NavigationService.Navigate<SupervisorInterviewViewModel, InterviewViewModelArgs>(
                new InterviewViewModelArgs
                {
                    InterviewId = interviewId,
                    NavigationIdentity = navigationIdentity
                });
        }

        protected override void FinishActivity() => TopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            TopActivity.Activity.StartActivity(new Intent(TopActivity.Activity, typeof(PrefsActivity)));
    }
}
