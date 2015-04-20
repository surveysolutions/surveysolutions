using System;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using MWBarcodeScanner;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class QRBarcodeQuestionView : AbstractQuestionView
    {
        private TextView qrBarcodeView;

        public QRBarcodeQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            var qrBarcodeViewParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent);
            qrBarcodeViewParams.SetMargins(0, 0, 150, 0);

            this.qrBarcodeView = new TextView(this.CurrentContext) {LayoutParameters = qrBarcodeViewParams};
            this.qrBarcodeView.SetTypeface(null, TypefaceStyle.Bold);

            this.InitializeViewAndButtonView(this.qrBarcodeView, "Scan", this.ScanQRBarcode);

            this.PutAnswerStoredInModelToUI();
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.qrBarcodeView.Text = this.GetAnswerStoredInModelAsString();
        }

        private async void ScanQRBarcode(object sender, EventArgs e)
        {
            var scanner = new Scanner(this.CurrentContext);
            
            customizeScanner();
            var result = await scanner.Scan();

            if (result == null) return;

            this.qrBarcodeView.Text = result.code;

            this.SaveAnswer(result.code,
                new AnswerQRBarcodeQuestionCommand(interviewId: this.QuestionnairePublicKey, userId: this.Membership.CurrentUser.Id,
                    questionId: this.Model.PublicKey.Id, rosterVector: this.Model.PublicKey.InterviewItemPropagationVector,
                    answerTime: DateTime.UtcNow, answer: result.code));
        }

        private void customizeScanner()
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
};