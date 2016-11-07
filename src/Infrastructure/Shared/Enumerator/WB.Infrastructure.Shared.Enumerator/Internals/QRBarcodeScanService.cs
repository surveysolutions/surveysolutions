using System.Threading.Tasks;
using MvvmCross.Platform.Droid.Platform;
using MWBarcodeScanner;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    internal class QRBarcodeScanService : IQRBarcodeScanService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        public QRBarcodeScanService(IMvxAndroidCurrentTopActivity androidCurrentTopActivity)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
        }

        public async Task<QRBarcodeScanResult> ScanAsync()
        {
            var scanner = new Scanner(this.androidCurrentTopActivity.Activity);
            scanner.setInterfaceOrientation(
                this.androidCurrentTopActivity.Activity.RequestedOrientation.ToString());

            this.CustomizeScanner();
            var result = await scanner.Scan();

            return result != null ? new QRBarcodeScanResult() {Code = result.code, RawBytes = result.bytes} : null;
        }

        private void CustomizeScanner()
        {
            BarcodeConfig.MWB_registerSDK("uaEgBR8jq/WJ7+CWNIP0iOEUYiTL0ayG6WGHMA2U6/U=");
        }
    }
}