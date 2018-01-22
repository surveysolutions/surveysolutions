using System;
using System.Drawing;
using System.Threading.Tasks;
using Android.App;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using MWBarcodeScanner;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal class QRBarcodeScanService : IQRBarcodeScanService
    {
        public static RectangleF RECT_FULL_1D = new RectangleF(6, 6, 88, 88);
        public static RectangleF RECT_FULL_2D = new RectangleF(20, 6, 60, 88);
        public static RectangleF RECT_DOTCODE = new RectangleF(30, 20, 40, 60);

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

            var activity = this.androidCurrentTopActivity.Activity;
            var scanner = new Scanner(activity);
            scanner.setInterfaceOrientation(
                activity.RequestedOrientation.ToString());

            this.CustomizeScanner(activity);
            var result = await scanner.Scan();

            return result != null ? new QRBarcodeScanResult() { Code = result.code, RawBytes = result.bytes } : null;
        }

        private void CustomizeScanner(Activity activity)
        {
            int registerResult = BarcodeConfig.MWB_registerSDK("uaEgBR8jq/WJ7+CWNIP0iOEUYiTL0ayG6WGHMA2U6/U=", activity);
            
            BarcodeConfig.MWB_setActiveCodes(BarcodeConfig.MWB_CODE_MASK_25 |
                                             BarcodeConfig.MWB_CODE_MASK_39 |
                                             BarcodeConfig.MWB_CODE_MASK_93 |
                                             BarcodeConfig.MWB_CODE_MASK_128 |
                                             BarcodeConfig.MWB_CODE_MASK_AZTEC |
                                             BarcodeConfig.MWB_CODE_MASK_DM |
                                             BarcodeConfig.MWB_CODE_MASK_EANUPC |
                                             BarcodeConfig.MWB_CODE_MASK_PDF |
                                             BarcodeConfig.MWB_CODE_MASK_QR |
                                             BarcodeConfig.MWB_CODE_MASK_CODABAR |
                                             BarcodeConfig.MWB_CODE_MASK_RSS |
                                             BarcodeConfig.MWB_CODE_MASK_MAXICODE |
                                             BarcodeConfig.MWB_CODE_MASK_POSTAL);

            BarcodeConfig.MWB_setDirection(BarcodeConfig.MWB_SCANDIRECTION_HORIZONTAL | BarcodeConfig.MWB_SCANDIRECTION_VERTICAL);
            BarcodeConfig.MWB_setLevel(2);

            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_25, RECT_FULL_1D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_39, RECT_FULL_1D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_93, RECT_FULL_1D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_128, RECT_FULL_1D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_AZTEC, RECT_FULL_2D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_DM, RECT_FULL_2D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_EANUPC, RECT_FULL_1D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_PDF, RECT_FULL_1D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_QR, RECT_FULL_2D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_RSS, RECT_FULL_1D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_CODABAR, RECT_FULL_1D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_DOTCODE, RECT_DOTCODE);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_MAXICODE, RECT_FULL_2D);
            BarcodeConfig.MWBsetScanningRect(BarcodeConfig.MWB_CODE_MASK_POSTAL, RECT_FULL_1D);

            BarcodeConfig.MWB_setMinLength(BarcodeConfig.MWB_CODE_MASK_25, 5);
            BarcodeConfig.MWB_setMinLength(BarcodeConfig.MWB_CODE_MASK_MSI, 5);
            BarcodeConfig.MWB_setMinLength(BarcodeConfig.MWB_CODE_MASK_39, 5);
            BarcodeConfig.MWB_setMinLength(BarcodeConfig.MWB_CODE_MASK_CODABAR, 5);
            BarcodeConfig.MWB_setMinLength(BarcodeConfig.MWB_CODE_MASK_11, 5);
            ScannerActivity.setParserMask(BarcodeConfig.MWP_PARSER_MASK_NONE);
        }
    }
}