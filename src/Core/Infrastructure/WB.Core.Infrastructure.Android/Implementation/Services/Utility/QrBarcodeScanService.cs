using System.Threading.Tasks;
using Cirrious.CrossCore.Droid.Platform;
using MWBarcodeScanner;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Utility
{
    internal class QrBarcodeScanService : IQrBarcodeScanService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        public QrBarcodeScanService(IMvxAndroidCurrentTopActivity androidCurrentTopActivity)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
        }

        public async Task<ScanResult> ScanAsync()
        {
            var scanner = new Scanner(this.androidCurrentTopActivity.Activity);

            this.CustomizeScanner();
            var result = await scanner.Scan();

            return result != null ? new ScanResult() {Code = result.code, RawBytes = result.bytes} : null;
        }

        private void CustomizeScanner()
        {
            BarcodeConfig.MWB_setDirection(BarcodeConfig.MWB_SCANDIRECTION_HORIZONTAL);

            //old codes not covered by new license
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_AZTEC, "litvin.roma@gmail.com", "7FC6AA87757EAFEE193B7D71C0A19C826ADFEDD0F64383D2F84D30157AC45688");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_RSS, "litvin.roma@gmail.com", "1F8366229BF68ADE7B9CA60C7668A100E786366FFC6482914E6F7205E6B85CCD");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_DM, "litvin.roma@gmail.com", "44360CC6838D0CB30206815D24BF39B2AC272E3A803059E13663344A6301EBEF");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_DOTCODE, "litvin.roma@gmail.com", "507654A980BE306DFFEB93975CFA6617C1FD0FAAEFE5CEFA4EDC15B7CF4DF7C0");

            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_QR, "WorldBank.QR.Android.UDL", "CBF549CA0E87FD8993F5868F8671CBD5894B5FC0AD3D42B66B3024F566ACC2BF");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_39, "WorldBank.C39.Android.UDL", "4BCECACD2C29819DBF9D6E9A736F13DD1EAAB6D7EA3C8BBC06801983C43D4249");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_93, "WorldBank.C93.Android.UDL", "0A7ADF070F4BD27822B8C19160B1BCA4CC6315104262D129536888DFE010838C");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_CODABAR, "WorldBank.CB.Android.UDL", "116402D44088A35DD228B350F0927C6057A503EA9B7D76DE210E872999B91372");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_25, "WorldBank.C25.Android.UDL", "25BAE398B9C1CD328F5D8AEA6F66910A9907CE7CAB0C6EC2FD51A44ABEC84751");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_128, "WorldBank.C128.Android.UDL", "17C7B32367D6364D87EED3B602BDD69DDCFE7F60E8BCE9DDC806BE22B4CCF2A5");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_PDF, "WorldBank.PDF.Android.UDL", "5530EEF1E9EA21110F826ED1403D5A2EA10EFD0BCA1B437AA19C2307004DFB7A");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_11, "WorldBank.C11.Android.UDL", "ACB00DE022A76521A0E92204C73230BE0B36344328905E387818D5419B61A046");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_MSI, "WorldBank.MSI.Android.UDL", "5E8FD2CF399B5C80A966182DEA98B8847D8ECBB262D22D2DF63E88EBBF4FCDA6");
        }
    }
}