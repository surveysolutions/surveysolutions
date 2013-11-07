using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class TextQuestionView : AbstractQuestionView
    {

        public TextQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source,
                                Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            etAnswer = new EditText(this.Context);
            etAnswer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                                   ViewGroup.LayoutParams.WrapContent);
            this.PutAnswerStoredInModelToUI();
            etAnswer.SetSelectAllOnFocus(true);
            etAnswer.ImeOptions = ImeAction.Done;
            etAnswer.SetSingleLine(true);
            etAnswer.EditorAction += etAnswer_EditorAction;
            etAnswer.FocusChange += etAnswer_FocusChange;
            llWrapper.Click += TextQuestionView_Click;
            llWrapper.AddView(etAnswer);
        }

        private void etAnswer_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                // ShowKeyboard(etAnswer);
                return;
            }

            var newAnswer = this.etAnswer.Text.Trim();

            if (newAnswer != this.Model.AnswerString)
            {
                if (!IsCommentsEditorFocused)
                    HideKeyboard(etAnswer);

                this.SaveAnswer(newAnswer,
                    new AnswerTextQuestionCommand(
                        this.QuestionnairePublicKey, CapiApplication.Membership.CurrentUser.Id, Model.PublicKey.Id,
                        this.Model.PublicKey.PropagationVector, DateTime.UtcNow, newAnswer));
            }
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.etAnswer.Text = this.GetAnswerStoredInModelAsString();
        }

        private void etAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            etAnswer.ClearFocus();
        }

        private void TextQuestionView_Click(object sender, EventArgs e)
        {
            etAnswer.RequestFocus();
            ShowKeyboard(etAnswer);
        }

        protected EditText etAnswer { get; set; }
    }
}