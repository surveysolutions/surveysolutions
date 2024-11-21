using Android.Content;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.CreateInterview;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ILogger log;

        public ViewModelNavigationService(
            ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IPrincipal principal,
            ILogger log)
            : base(commandService,
                userInteractionService,
                userInterfaceStateService,
                principal,
                log)
        {
            this.log = log;
        }

        public override async Task<bool> NavigateToDashboardAsync(string interviewId = null)
        {
            this.log.Trace($"Navigating to dashboard interviewId: {interviewId ?? "'null'"}");
            if (interviewId == null)
            {
               return await NavigateToAsync<DashboardViewModel>().ConfigureAwait(false);
            }

            return await NavigateToAsync<DashboardViewModel, DashboardViewModelArgs>(new DashboardViewModelArgs
            {
                InterviewId = Guid.Parse(interviewId)
            }).ConfigureAwait(false);
        }

        public override async Task<bool> NavigateToPrefilledQuestionsAsync(string interviewId)
        {
            this.log.Trace($"Navigating to PrefilledQuestionsViewModel interviewId: {interviewId}");
            return await NavigateToAsync<InterviewViewModel, InterviewViewModelArgs>(
                new InterviewViewModelArgs
                {
                    InterviewId = interviewId,
                    NavigationIdentity = NavigationIdentity.CreateForCoverScreen(),
                });
        }

        public override void NavigateToSplashScreen()
        {
            base.RestartApp(typeof(SplashActivity));
        }

        public override Task NavigateToFinishInstallationAsync()
        {
            this.log.Trace("Navigating to FinishInstallationViewModel");
            return this.NavigateToAsync<FinishInstallationViewModel>();
        }

        public override Task NavigateToMapsAsync()
        {
            this.log.Trace("Navigating to MapsViewModel");
            return this.NavigateToAsync<MapsViewModel>();
        }

        public override async Task<bool> NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity, 
            SourceScreen sourceScreen)
        {
            this.log.Trace($"Navigating to interview {interviewId}:{navigationIdentity}");
            return await NavigateToAsync<InterviewViewModel, InterviewViewModelArgs>(
                new InterviewViewModelArgs
                {
                    InterviewId = interviewId,
                    NavigationIdentity = navigationIdentity,
                    SourceScreen = sourceScreen,
                });
        }

        public override Task NavigateToCreateAndLoadInterview(int assignmentId, SourceScreen sourceScreen)
        {
            return this.NavigateToAsync<CreateAndLoadInterviewViewModel, CreateInterviewViewModelArg>(
                new CreateInterviewViewModelArg()
                {
                    AssignmentId = assignmentId,
                    InterviewId = Guid.NewGuid(),
                    SourceScreen = sourceScreen,
                }, true);
        }

        public override Task NavigateToLoginAsync()
        {
            this.log.Trace("Navigating to login");
            return this.NavigateToAsync<LoginViewModel>();
        }

        protected override void NavigateToSettingsImpl() =>
            TopActivity.Activity.StartActivity(new Intent(TopActivity.Activity, typeof(PrefsActivity)));
    }
}
