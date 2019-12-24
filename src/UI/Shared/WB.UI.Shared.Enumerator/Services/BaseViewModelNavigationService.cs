using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Java.Lang;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using MvvmCross.Plugin.Messenger;
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
        private readonly IMvxMessenger messenger;
        private readonly ILogger logger;
        private MvxSubscriptionToken token;

        protected BaseViewModelNavigationService(ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity topActivity,
            IMvxNavigationService navigationService,
            IPrincipal principal, 
            ILogger logger, IMvxMessenger messenger)
        {
            this.commandService = commandService;
            this.userInteractionService = userInteractionService;
            this.userInterfaceStateService = userInterfaceStateService;
            this.topActivity = topActivity;
            this.navigationService = navigationService;
            this.principal = principal;
            this.logger = logger;
            this.messenger = messenger;
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

        public Task EnsureHasPermissionToInstallFromUnknownSourcesAsync()
        {
            // Check if application has permission to do package installations
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O && !Application.Context.PackageManager.CanRequestPackageInstalls())
            {
                // if not - open settings menu for current application
                var addFlags = ShareCompat.IntentBuilder.From(this.topActivity.Activity)
                    .Intent
                    .SetAction(Android.Provider.Settings.ActionManageUnknownAppSources)
                    .SetData(Android.Net.Uri.Parse("package:" + Application.Context.PackageName))
                    .AddFlags(ActivityFlags.NewTask);

                Application.Context.StartActivity(addFlags);

                // prepare async action, there is no <void> version for TaskCompletionSource, using bool
                var tcs = new TaskCompletionSource<bool>();

                // subscribing on Application resume event
                this.token = messenger.Subscribe<ApplicationResumeMessage>(m =>
                {
                    try
                    {
                        // double check for permission to do package installations
                        if (!Application.Context.PackageManager.CanRequestPackageInstalls())
                        {
                            // set update error. This message will be displayed on user UI
                            tcs.SetException(new System.Exception(UIResources.ErrorMessage_PermissionForUnkownSourcesRequired));
                        }
                        else
                        {
                            // we have all permissions to install application, proceed with next actions
                            tcs.SetResult(true);
                        }
                    }
                    finally
                    {   
                        // cleaning up subscription token
                        messenger.Unsubscribe<ApplicationResumeMessage>(token);
                    }
                });

                return tcs.Task;
            }

            // we have all permissions to install application, proceed with next actions
            return Task.FromResult(true);
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
