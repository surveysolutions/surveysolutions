using System;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using MWBarcodeScanner;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
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
            var scanner = new Scanner(this.Context);
            var result = await scanner.Scan();

            if (result == null) return;

            this.qrBarcodeView.Text = result.code;

            this.SaveAnswer(result.code,
                new AnswerQRBarcodeQuestionCommand(interviewId: this.QuestionnairePublicKey, userId: this.Membership.CurrentUser.Id,
                    questionId: this.Model.PublicKey.Id, rosterVector: this.Model.PublicKey.InterviewItemPropagationVector,
                    answerTime: DateTime.UtcNow, answer: result.code));
        }
    }
};