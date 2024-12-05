using System.Reflection;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.Lang;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Attributes;
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
        private readonly IPrincipal principal;
        private readonly ILogger logger;

        protected IMvxAndroidCurrentTopActivity TopActivity { get; }
        private IMvxNavigationService NavigationService { get; }

        protected BaseViewModelNavigationService(ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IPrincipal principal, 
            ILogger logger)
        {
            this.commandService = commandService;
            this.userInteractionService = userInteractionService;
            this.userInterfaceStateService = userInterfaceStateService;
            TopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            NavigationService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
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
            return this.NavigationService.Close(viewModel);
        }
        
        public void InstallNewApp(string pathToApk)
        {
            this.logger.Info($"Installing new app {pathToApk} android build version code {Build.VERSION.SdkInt}");
            Intent promptInstall;
            
            var topActivity = TopActivity;
            
            var uriForFile = FileProvider.GetUriForFile(topActivity.Activity.BaseContext,
                topActivity.Activity.ApplicationContext.PackageName + ".fileprovider",
                new Java.IO.File(pathToApk));

            promptInstall = ShareCompat.IntentBuilder.From(topActivity.Activity)
                .SetStream(uriForFile)
                .Intent
                .SetAction(Intent.ActionView)
                .SetDataAndType(uriForFile, "application/vnd.android.package-archive")
                .AddFlags(ActivityFlags.GrantReadUriPermission)
                .AddFlags(ActivityFlags.NewTask);
            

            Application.Context.StartActivity(promptInstall);
        }

        public void CloseApplication() => JavaSystem.Exit(0);
        public abstract Task NavigateToCreateAndLoadInterview(int assignmentId);

        public abstract Task NavigateToLoginAsync();
        public abstract Task NavigateToFinishInstallationAsync();

        public abstract Task NavigateToMapsAsync();

        public abstract Task<bool> NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity);
        public abstract Task<bool> NavigateToPrefilledQuestionsAsync(string interviewId);
        public abstract void NavigateToSplashScreen();

        protected abstract void NavigateToSettingsImpl();

        public async Task<bool> NavigateToAsync<TViewModel, TParam>(TParam param, bool finishActivityOnSuccess = false) 
            where TViewModel : IMvxViewModel<TParam>
        {
            if (this.HasPendingOperations)
            {
                this.logger.Trace($"Prevent navigate to {typeof(TViewModel)} with {typeof(TParam)} because answering in progress ");
                this.ShowWaitMessage();
                return false;
            }

            this.logger.Trace($"Navigate to new {typeof(TViewModel)} with {typeof(TParam)}");
            var previousActivity = TopActivity.Activity;
            SaveInterviewExitPoint(typeof(TViewModel), param);
            var result = await this.NavigationService.Navigate<TViewModel, TParam>(param);
            if(result && finishActivityOnSuccess)
                previousActivity.Finish();
            
            return result;
        }

        public Task<bool> NavigateToAsync<TViewModel>() where TViewModel : IMvxViewModel
        {
            if (!this.HasPendingOperations)
            {
                SaveInterviewExitPoint(typeof(TViewModel), null);
                return this.NavigationService.Navigate<TViewModel>();
            }
            
            this.ShowWaitMessage();
            return Task.FromResult(false);

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

            var currentActivity = TopActivity.Activity;
            var intent = new Intent(currentActivity, splashScreenType);
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            currentActivity.StartActivity(intent);
            currentActivity.Finish();
        }

        public virtual void NavigateToSystemDateSettings()
        {
            var currentActivity = TopActivity.Activity;
            var intent = new Intent(Android.Provider.Settings.ActionDateSettings);
            currentActivity.StartActivity(intent);
        }

        private static Tuple<Type, object> sourceOfInterview = null;
        
        public Task<bool> NavigateFromInterviewAsync(string interviewId = null)
        {
            if (sourceOfInterview != null)
            {
                var viewModelType = sourceOfInterview.Item1;
                var param = sourceOfInterview.Item2;
                MethodInfo method = typeof(BaseViewModelNavigationService)
                    .GetMethod(nameof(NavigateToAsyncImpl), BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo genericMethod = method.MakeGenericMethod(viewModelType, param.GetType());
                return (Task<bool>)genericMethod.Invoke(this, [param]);
            }
            else
                return this.NavigateToDashboardAsync(interviewId);
        }
        
        private void SaveInterviewExitPoint(Type viewModelType, object param)
        {
            var attributes = viewModelType.GetCustomAttributes<InterviewEntryPointAttribute>();
            if (attributes.Any())
                sourceOfInterview = new Tuple<Type, object>(viewModelType, param);
        }
        
        private Task<bool> NavigateToAsyncImpl<TViewModel, TParam>(TParam param)
            where TViewModel : IMvxViewModel<TParam>
        {
            return NavigateToAsync<TViewModel, TParam>(param);
        }
    }
}
