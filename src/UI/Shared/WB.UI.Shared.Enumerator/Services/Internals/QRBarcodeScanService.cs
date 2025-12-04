using System.Threading.Tasks;
using Android.App;
using Android.Content;
using MvvmCross;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Activities;
using Xamarin.Essentials;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal class QRBarcodeScanService : IQRBarcodeScanService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IPermissionsService permissions;
        private static TaskCompletionSource<QRBarcodeScanResult> scanTaskCompletionSource;

        public QRBarcodeScanService(IPermissionsService permissions)
        {
            this.androidCurrentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            this.permissions = permissions;
        }

        public async Task<QRBarcodeScanResult> ScanAsync()
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.Camera>().ConfigureAwait(false);
            
            scanTaskCompletionSource = new TaskCompletionSource<QRBarcodeScanResult>();

            var activity = this.androidCurrentTopActivity.Activity;
            var intent = new Intent(activity, typeof(BarcodeScannerActivity));
            
            activity.StartActivity(intent);

            var result = await scanTaskCompletionSource.Task;
            return result;
        }

        public static void SetResult(QRBarcodeScanResult result)
        {
            scanTaskCompletionSource?.TrySetResult(result);
        }
    }
}
