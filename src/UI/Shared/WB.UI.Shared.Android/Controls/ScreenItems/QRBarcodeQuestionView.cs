using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using ZXing.Mobile;

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

            this.qrBarcodeView = new TextView(this.Context);
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
            var scanner = new MobileBarcodeScanner(this.Context)
            {
                TopText = "Hold the camera up to the barcode\nAbout 6 inches away",
                BottomText = "Wait for the barcode to automatically scan!"
            };

            var result = await scanner.Scan();

            if (result == null) return;

            this.qrBarcodeView.Text = result.Text;

            this.SaveAnswer(result.Text,
                new AnswerQRBarcodeQuestionCommand(interviewId: this.QuestionnairePublicKey, userId: this.Membership.CurrentUser.Id,
                    questionId: this.Model.PublicKey.Id, rosterVector: this.Model.PublicKey.InterviewItemPropagationVector,
                    answerTime: DateTime.UtcNow, answer: result.Text));
        }
    }
};