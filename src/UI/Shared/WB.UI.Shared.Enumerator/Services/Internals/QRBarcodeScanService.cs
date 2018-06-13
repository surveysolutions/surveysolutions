using System.Threading.Tasks;
using MvvmCross.Platforms.Android;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using ZXing.Mobile;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal class QRBarcodeScanService : IQRBarcodeScanService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IPermissions permissions;

        public QRBarcodeScanService(IMvxAndroidCurrentTopActivity androidCurrentTopActivity, IPermissions permissions)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.permissions = permissions;
        }

        public async Task<QRBarcodeScanResult> ScanAsync()
        {
            await this.permissions.AssureHasPermission(Permission.Camera);
            
            MobileBarcodeScanner.Initialize(this.androidCurrentTopActivity.Activity.Application);
            var scanner = new MobileBarcodeScanner();
            var result = await scanner.Scan();

            return result != null ? new QRBarcodeScanResult() { Code = result.Text, RawBytes = result.RawBytes} : null;
        }
    }
}
