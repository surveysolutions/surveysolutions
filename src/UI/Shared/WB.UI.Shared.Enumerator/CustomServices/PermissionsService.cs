using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Platforms.Android;
using MvvmCross.Plugin.Messenger;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.CustomPermissions;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.UI.Shared.Enumerator.Services;
using Xamarin.Essentials;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class PermissionsService : IPermissionsService
    {
        private readonly IMvxMainThreadAsyncDispatcher asyncDispatcher;
        private readonly IMvxAndroidCurrentTopActivity currentTopActivity;

        public PermissionsService(IMvxMainThreadAsyncDispatcher asyncDispatcher)
        {
            this.asyncDispatcher = asyncDispatcher;
            this.currentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
        }

        private MvxSubscriptionToken token;

        public async Task AssureHasPermissionOrThrow<T>() where T : Permissions.BasePermission, new()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M) return;
            if (await Permissions.CheckStatusAsync<T>().ConfigureAwait(false) == PermissionStatus.Granted) return;

            if (asyncDispatcher.IsOnMainThread)
            {
                await RequestPermission<T>();
            }
            else
            {
                await asyncDispatcher.ExecuteOnMainThreadAsync(RequestPermission<T>, maskExceptions: false);
            }
        }

        private static async Task RequestPermission<T>() where T : Permissions.BasePermission, new()
        {
            var permissionsRequest = await Permissions.RequestAsync<T>().ConfigureAwait(false);
            if (permissionsRequest != PermissionStatus.Granted)
                throw new MissingPermissionsException(UIResources.MissingPermission, typeof(T));
        }

        public Task EnsureHasPermissionToInstallFromUnknownSourcesAsync()
        {
            // Check if application has permission to do package installations
#pragma warning disable CA1416 // Validate platform compatibility
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O && !Application.Context.PackageManager.CanRequestPackageInstalls())
            {
                // if not - open settings menu for current application
                var addFlags = ShareCompat.IntentBuilder.From(this.currentTopActivity.Activity)
                    .Intent
                    .SetAction(Android.Provider.Settings.ActionManageUnknownAppSources)
                    .SetData(Android.Net.Uri.Parse("package:" + Application.Context.PackageName))
                    .AddFlags(ActivityFlags.NewTask);

                Application.Context.StartActivity(addFlags);

                // prepare async action, there is no <void> version for TaskCompletionSource, using bool
                var tcs = new TaskCompletionSource<bool>();
                
                var messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
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
#pragma warning restore CA1416 // Validate platform compatibility

            // we have all permissions to install application, proceed with next actions
            return Task.FromResult(true);
        }

        public async Task<PermissionStatus> CheckPermissionStatusAsync<T>() where T : Permissions.BasePermission, new()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M) return PermissionStatus.Unknown;
            return await Permissions.CheckStatusAsync<T>().ConfigureAwait(false);
        }

        public Task AssureHasManageExternalStoragePermission()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.R)
            {
                return this.AssureHasPermissionOrThrow<Permissions.StorageWrite>();
            }

            var status = Android.OS.Environment.IsExternalStorageManager;
            if (status)
                return Task.FromResult(true);
            
            var askManageFiles = ShareCompat.IntentBuilder.From(this.currentTopActivity.Activity)
                .Intent
                .SetAction(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission)
                .SetData(Android.Net.Uri.Parse("package:" + Application.Context.PackageName))
                .AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(askManageFiles);

            var tcs = new TaskCompletionSource<bool>();
                
            var messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();

            this.token = messenger.Subscribe<ApplicationResumeMessage>(m =>
            {
                try
                {
                    var newStatus =  Android.OS.Environment.IsExternalStorageManager;
                    if (newStatus)
                    {
                        tcs.SetResult(true);
                    }
                    else
                    {
                        tcs.SetException(new MissingPermissionsException(UIResources.MissingPermission,
                            manageExternalStoragePermission.GetType()));
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
    }
}
