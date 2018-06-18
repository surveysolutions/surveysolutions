using System;
using System.Threading.Tasks;
using Android.Content;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Services
{
    public abstract class BaseViewModelNavigationService : IViewModelNavigationService
    {
        private readonly ICommandService commandService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IUserInterfaceStateService userInterfaceStateService;
        private readonly IMvxAndroidCurrentTopActivity topActivity;
        private readonly IMvxNavigationService navigationService;
        private readonly IPrincipal principal;

        protected BaseViewModelNavigationService(ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity topActivity,
            IMvxNavigationService navigationService,
            IPrincipal principal)
        {
            this.commandService = commandService;
            this.userInteractionService = userInteractionService;
            this.userInterfaceStateService = userInterfaceStateService;
            this.topActivity = topActivity;
            this.navigationService = navigationService;
            this.principal = principal;
        }

        public virtual bool HasPendingOperations => this.commandService.HasPendingCommands ||
                                                    this.userInteractionService.HasPendingUserInterations ||
                                                    this.userInterfaceStateService.IsUserInferfaceLocked;

        public abstract Task NavigateToLoginAsync();
        public abstract Task NavigateToFinishInstallationAsync();

        public abstract Task NavigateToMapsAsync();


        public abstract Task NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity);
        public abstract Task NavigateToPrefilledQuestionsAsync(string interviewId);
        public abstract void NavigateToSplashScreen();

        protected abstract void FinishActivity();

        protected abstract void NavigateToSettingsImpl();

        public Task NavigateToAsync<TViewModel, TParam>(TParam param) where TViewModel : IMvxViewModel<TParam>
        {
            if (this.HasPendingOperations)
            {
                this.ShowWaitMessage();
                return Task.CompletedTask;
            }
            else
                return this.navigationService.Navigate<TViewModel, TParam>(param);
        }

        public Task NavigateToAsync<TViewModel>() where TViewModel : IMvxViewModel
        {
            if (this.HasPendingOperations)
            {
                this.ShowWaitMessage();
                return Task.CompletedTask;
            }

            return this.navigationService.Navigate<TViewModel>();
        }

        public abstract Task NavigateToDashboardAsync(string interviewId = null);

        public async Task SignOutAndNavigateToLoginAsync()
        {
            if (this.HasPendingOperations)
                this.ShowWaitMessage();
            else
            {
                this.principal.SignOut();
                await this.NavigateToLoginAsync().ConfigureAwait(false);
                this.FinishActivity();
            }
        }

        public virtual void NavigateToSettings()
        {
            if (this.HasPendingOperations)
                this.ShowWaitMessage();
            else
                this.NavigateToSettingsImpl();
        }

        public void ShowWaitMessage()
            =>  this.userInteractionService.ShowToast(UIResources.Messages_WaitPendingOperation);

        protected void RestartApp(Type splashScreenType)
        {
            var currentActivity = topActivity.Activity;
            var intent = new Intent(currentActivity, splashScreenType);
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            currentActivity.StartActivity(intent);
            currentActivity.Finish();
        }
    }
}
