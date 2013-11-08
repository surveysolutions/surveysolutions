using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.UI.Capi.Shared.Controls.ScreenItems
{
    public class TextQuestionView : AbstractQuestionView
    {

        public TextQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source,
                                Guid questionnairePublicKey,  
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.etAnswer = new EditText(this.Context);
            this.etAnswer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                                   ViewGroup.LayoutParams.WrapContent);
            this.PutAnswerStoredInModelToUI();
            this.etAnswer.SetSelectAllOnFocus(true);
            this.etAnswer.ImeOptions = ImeAction.Done;
            this.etAnswer.SetSingleLine(true);
            this.etAnswer.EditorAction += this.etAnswer_EditorAction;
            this.etAnswer.FocusChange += this.etAnswer_FocusChange;
            this.llWrapper.Click += this.TextQuestionView_Click;
            this.llWrapper.AddView(this.etAnswer);
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
                if (!this.IsCommentsEditorFocused)
                    this.HideKeyboard(this.etAnswer);

                this.SaveAnswer(newAnswer,
                    new AnswerTextQuestionCommand(
                        this.QuestionnairePublicKey, this.Membership.CurrentUser.Id, this.Model.PublicKey.Id,
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
            this.etAnswer.ClearFocus();
        }

        private void TextQuestionView_Click(object sender, EventArgs e)
        {
            this.etAnswer.RequestFocus();
            this.ShowKeyboard(this.etAnswer);
        }

        protected EditText etAnswer { get; set; }
    }
}