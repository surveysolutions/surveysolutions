using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class TextQuestionView : AbstractQuestionView
    {

        public TextQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source,questionnairePublicKey)
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
        void etAnswer_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                ShowKeyboard(etAnswer);
                return;
            }
            SaveAnswer();
            if (!IsCommentsEditorFocused)
                HideKeyboard(etAnswer);
         /*   InputMethodManager imm
               = (InputMethodManager)this.Context.GetSystemService(
                   Context.InputMethodService);
            if (imm.IsAcceptingText)
            {
                SaveAnswer();
                if (!IsCommentsEditorFocused)
                    HideKeyboard(etAnswer);
            }*/
        }
        protected override void SaveAnswer()
        {
            string newValue = etAnswer.Text.Trim();
            if (newValue != this.Model.AnswerString)
            {
                CommandService.Execute(new AnswerTextQuestionCommand(this.QuestionnairePublicKey, CapiApplication.Membership.CurrentUser.Id,Model.PublicKey.PublicKey,
                                                     null,DateTime.Now, newValue));
            }
        
            base.SaveAnswer();
        }
        void etAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            etAnswer.ClearFocus();
        }

        void TextQuestionView_Click(object sender, EventArgs e)
        {
            etAnswer.RequestFocus();
        }


        protected EditText etAnswer { get; set; }

       
    }
}