using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using MvvmCross;
using MvvmCross.Platforms.Android;
using MvvmCross.Plugin.Messenger;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class PermissionsService : IPermissionsService
    {
        private readonly IPermissions permissions;
        private MvxSubscriptionToken token;

        public PermissionsService(IPermissions permissions)
        {
            this.permissions = permissions;
        }

        public async Task AssureHasPermissionOrThrow<T>() where T : BasePermission, new() 
            => await this.permissions.AssureHasPermissionOrThrow<T>();

        public Task EnsureHasPermissionToInstallFromUnknownSourcesAsync()
        {
            // Check if application has permission to do package installations
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O && !Application.Context.PackageManager.CanRequestPackageInstalls())
            {
                IMvxAndroidCurrentTopActivity topActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
                // if not - open settings menu for current application
                var addFlags = ShareCompat.IntentBuilder.From(topActivity.Activity)
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

            // we have all permissions to install application, proceed with next actions
            return Task.FromResult(true);
        }

    }
}
