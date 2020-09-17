using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.Lang;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ILogger logger;

        protected BaseViewModelNavigationService(ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity topActivity,
            IMvxNavigationService navigationService,
            IPrincipal principal, 
            ILogger logger)
        {
            this.commandService = commandService;
            this.userInteractionService = userInteractionService;
            this.userInterfaceStateService = userInterfaceStateService;
            this.topActivity = topActivity;
            this.navigationService = navigationService;
            this.principal = principal;
            this.logger = logger;
        }

        public virtual bool HasPendingOperations => 
            this.commandService.HasPendingCommands ||
            this.userInteractionService.HasPendingUserInteractions ||
            this.userInterfaceStateService.IsUserInterfaceLocked ||
            this.userInterfaceStateService.HasPendingThrottledActions;

        public Task Close(IMvxViewModel viewModel)
        {
            this.logger.Trace($"Closing viewmodel {viewModel.GetType()}");
            return this.navigationService.Close(viewModel);
        }
        
        public void InstallNewApp(string pathToApk)
        {
            this.logger.Info($"Installing new app {pathToApk} android build version code {Build.VERSION.SdkInt}");
            Intent promptInstall;

            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
            {
                promptInstall =
                    new Intent(Intent.ActionView)
                        .SetDataAndType(Android.Net.Uri.FromFile(new Java.IO.File(pathToApk)), "application/vnd.android.package-archive")
                        .AddFlags(ActivityFlags.NewTask)
                        .AddFlags(ActivityFlags.GrantReadUriPermission);
            }
            else 
            {
                var uriForFile = FileProvider.GetUriForFile(this.topActivity.Activity.BaseContext,
                    this.topActivity.Activity.ApplicationContext.PackageName + ".fileprovider",
                    new Java.IO.File(pathToApk));

                promptInstall = ShareCompat.IntentBuilder.From(this.topActivity.Activity)
                    .SetStream(uriForFile)
                    .Intent
                    .SetAction(Intent.ActionView)
                    .SetDataAndType(uriForFile, "application/vnd.android.package-archive")
                    .AddFlags(ActivityFlags.GrantReadUriPermission)
                    .AddFlags(ActivityFlags.NewTask);
            }

            Application.Context.StartActivity(promptInstall);
        }

        public void CloseApplication() => JavaSystem.Exit(0);
        public abstract Task NavigateToCreateAndLoadInterview(int assignmentId);

        public abstract Task NavigateToLoginAsync();
        public abstract Task NavigateToFinishInstallationAsync();

        public abstract Task NavigateToMapsAsync();

        public abstract Task NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity);
        public abstract Task NavigateToPrefilledQuestionsAsync(string interviewId);
        public abstract void NavigateToSplashScreen();

        protected abstract void FinishActivity();

        protected abstract void NavigateToSettingsImpl();

        public Task<bool> NavigateToAsync<TViewModel, TParam>(TParam param) where TViewModel : IMvxViewModel<TParam>
        {
            if (this.HasPendingOperations)
            {
                this.logger.Trace($"Prevent navigate to {typeof(TViewModel)} with {typeof(TParam)} because answering in progress ");
                this.ShowWaitMessage();
                return Task.FromResult(false);
            }

            this.logger.Trace($"Navigate to new {typeof(TViewModel)} with {typeof(TParam)}");
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

        public abstract Task<bool> NavigateToDashboardAsync(string interviewId = null);

        public async Task SignOutAndNavigateToLoginAsync()
        {
            if (this.HasPendingOperations)
                this.ShowWaitMessage();
            else
            {
                this.principal.SignOut();
                await this.NavigateToLoginAsync();
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
            this.logger.Trace("RestartApp");

            var currentActivity = topActivity.Activity;
            var intent = new Intent(currentActivity, splashScreenType);
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            currentActivity.StartActivity(intent);
            currentActivity.Finish();
        }
    }
}
