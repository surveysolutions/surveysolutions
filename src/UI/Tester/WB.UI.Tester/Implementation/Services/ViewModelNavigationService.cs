using System;
using System.Threading.Tasks;
using Android.Content;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Tester.Activities;

namespace WB.UI.Tester.Implementation.Services
{
    public class ViewModelNavigationService : BaseViewModelNavigationService, IViewModelNavigationService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IJsonAllTypesSerializer jsonSerializer;

        public ViewModelNavigationService(
            ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity androidCurrentTopActivity,
            IPrincipal principal,
            IJsonAllTypesSerializer jsonSerializer)
            : base(commandService, userInteractionService, userInterfaceStateService, androidCurrentTopActivity,principal)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.jsonSerializer = jsonSerializer;
        }

        public void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel => this.NavigateTo<TViewModel>(null);

        public Task NavigateToDashboard(Guid? interviewId = null)
        {
            this.NavigateTo<DashboardViewModel>();
            return Task.CompletedTask;
        }

        public void NavigateToSplashScreen()
        {
            base.RestartApp(typeof(SplashActivity));
        }

        public void NavigateToPrefilledQuestions(string interviewId) => this.NavigateTo<PrefilledQuestionsViewModel>(new { interviewId = interviewId });

        public void NavigateToInterview(string interviewId, NavigationIdentity navigationIdentity)
            => this.NavigateTo<InterviewViewModel>(new
            {
                interviewId = interviewId,
                jsonNavigationIdentity = navigationIdentity != null ? this.jsonSerializer.Serialize(navigationIdentity) : null
            });

        public override void NavigateToLogin() => this.NavigateTo<LoginViewModel>();
        protected override void FinishActivity() => this.androidCurrentTopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            this.androidCurrentTopActivity.Activity.StartActivity(new Intent(this.androidCurrentTopActivity.Activity, typeof(PrefsActivity)));
    }
}