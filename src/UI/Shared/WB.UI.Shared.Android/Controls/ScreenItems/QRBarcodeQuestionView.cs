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
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_39, "litvin.roma@gmail.com", "C93A74ABF3B014829C71BFB6DA0BC4FBCFCF52319211743311C75CE629A2A1D5");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_93, "litvin.roma@gmail.com", "5E7B8E0993565F6CBD272035F8ADE949B4386F531517ADC857CF2E22092C867C");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_25, "litvin.roma@gmail.com", "CBA4FA24D1A7BC8F9DEE920439A8165720C66968FB20180C19AF50906C7BC6FB");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_128, "litvin.roma@gmail.com", "0104680BFA0B26D155529E81A400D4B402B7D953685AF0F368AC924177F6DB44");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_AZTEC, "litvin.roma@gmail.com", "7FC6AA87757EAFEE193B7D71C0A19C826ADFEDD0F64383D2F84D30157AC45688");
            //BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_EANUPC, "litvin.roma@gmail.com", "key");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_QR, "litvin.roma@gmail.com", "8A4F3E2F6DF1580E74317C9B16338703E7748CBCCE2F118B7573A3BF437F2591");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_PDF, "litvin.roma@gmail.com", "2A0DF2A1BA4C30B4691A91E8CA753E1A8D329D56490EEA33A6BEBD06F0058BF9");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_RSS, "litvin.roma@gmail.com", "1F8366229BF68ADE7B9CA60C7668A100E786366FFC6482914E6F7205E6B85CCD");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_CODABAR, "litvin.roma@gmail.com", "1F8E4B902E76705AAB1F8370A6C254E46C1D210E032E15AA6A9A945B3630198A");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_DM, "litvin.roma@gmail.com", "44360CC6838D0CB30206815D24BF39B2AC272E3A803059E13663344A6301EBEF");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_DOTCODE, "litvin.roma@gmail.com", "507654A980BE306DFFEB93975CFA6617C1FD0FAAEFE5CEFA4EDC15B7CF4DF7C0");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_11, "litvin.roma@gmail.com", "A149CEE60D470E291546422E4A1A823654A706EDF82CD47599B479F0A34CD18B");
            BarcodeConfig.MWB_registerCode(BarcodeConfig.MWB_CODE_MASK_MSI, "litvin.roma@gmail.com", "E469FCA82AF1E6FAA3E24E4280DC0C21AD2E810072D5DE145E60297161076071");
        }
    }
};