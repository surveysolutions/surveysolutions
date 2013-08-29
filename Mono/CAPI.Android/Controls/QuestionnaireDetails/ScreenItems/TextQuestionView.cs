using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;

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
            etAnswer.Text = Model.AnswerString;
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
            SaveAnswer();
            /* if (!IsCommentsEditorFocused)
                 HideKeyboard(etAnswer);*/
        }

        protected override void SaveAnswer()
        {
            string newValue = etAnswer.Text.Trim();
            if (newValue != this.Model.AnswerString)
            {
                ExecuteSaveAnswerCommand(new SetAnswerCommand(this.QuestionnairePublicKey, Model.PublicKey.PublicKey,
                                                              null, newValue,
                                                              Model.PublicKey.PropagationKey));
                if (!IsCommentsEditorFocused)
                    HideKeyboard(etAnswer);
            }

            base.SaveAnswer();
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