using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using Xamarin.Essentials;
using ZXing.Mobile;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal class QRBarcodeScanService : IQRBarcodeScanService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IPermissionsService permissions;

        public QRBarcodeScanService(IPermissionsService permissions)
        {
            this.androidCurrentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            this.permissions = permissions;
        }

        public async Task<QRBarcodeScanResult> ScanAsync()
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.Camera>().ConfigureAwait(false);
            
            MobileBarcodeScanner.Initialize(this.androidCurrentTopActivity.Activity.Application);
            var scanner = new MobileBarcodeScanner();
            var result = await scanner.Scan();

            return result != null ? new QRBarcodeScanResult() { Code = result.Text, RawBytes = result.RawBytes} : null;
        }
    }
}
