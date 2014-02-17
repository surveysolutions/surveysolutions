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

            #region create ui controls and put them to the parent control

            // place holder for text control and button
            var wrapper = new GridLayout(this.Context)
            {
                ColumnCount = 2,
                LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.FillParent,
                    ViewGroup.LayoutParams.FillParent)
            };

            var wrapContenLayout = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                               ViewGroup.LayoutParams.WrapContent);
            wrapContenLayout.AddRule(LayoutRules.AlignParentRight);

            this.qrBarcodeView = new TextView(this.Context);
            this.qrBarcodeView.SetTypeface(null, TypefaceStyle.Bold);

            var scanButton = new Button(this.Context) { Text = "Scan", LayoutParameters = wrapContenLayout };
            scanButton.Click += this.ScanQRBarcode;

            wrapper.AddView(scanButton);
            wrapper.AddView(this.qrBarcodeView);

            this.llWrapper.AddView(wrapper);
            #endregion

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