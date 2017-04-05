using System.Threading.Tasks;
using MvvmCross.Platform.Droid.Platform;
using MWBarcodeScanner;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Infrastructure.Shared.Enumerator.Internals
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

            var scanner = new Scanner(this.androidCurrentTopActivity.Activity);
            scanner.setInterfaceOrientation(
                this.androidCurrentTopActivity.Activity.RequestedOrientation.ToString());

            this.CustomizeScanner();
            var result = await scanner.Scan();

            return result != null ? new QRBarcodeScanResult() { Code = result.code, RawBytes = result.bytes } : null;
        }

        private void CustomizeScanner()
        {
            BarcodeConfig.MWB_registerSDK("uaEgBR8jq/WJ7+CWNIP0iOEUYiTL0ayG6WGHMA2U6/U=");
        }
    }
}