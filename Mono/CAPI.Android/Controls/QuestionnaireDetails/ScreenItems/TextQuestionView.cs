using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Main.Core.Commands.Questionnaire.Completed;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class TextQuestionView : AbstractQuestionView
    {
      /*  public TextQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }*/

        public TextQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source,questionnairePublicKey)
        {
        }

     /*   public TextQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model) : base(context, attrs, defStyle, model)
        {
        }

        public TextQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model) : base(javaReference, transfer, model)
        {
        }*/

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
                return;
            InputMethodManager imm
               = (InputMethodManager)this.Context.GetSystemService(
                   Context.InputMethodService);
            if (imm.IsAcceptingText)
            {
                SaveAnswer();
                imm.HideSoftInputFromWindow(etAnswer.WindowToken, 0);
            }
        }
        protected void SaveAnswer()
        {
            CommandService.Execute(new SetAnswerCommand(this.QuestionnairePublicKey, Model.PublicKey.PublicKey,
                                                      null, etAnswer.Text,
                                                      Model.PublicKey.PropagationKey));
        }
        void etAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            etAnswer.ClearFocus();
        }

        void TextQuestionView_Click(object sender, EventArgs e)
        {
            etAnswer.RequestFocus();
            InputMethodManager imm
               = (InputMethodManager)this.Context.GetSystemService(
                   Context.InputMethodService);
            imm.ShowSoftInput(etAnswer, 0);
        }


        protected EditText etAnswer { get; set; }

       
    }
}