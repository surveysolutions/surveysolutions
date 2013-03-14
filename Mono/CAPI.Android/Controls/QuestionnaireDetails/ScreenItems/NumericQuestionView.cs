using System;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Main.Core.Commands.Questionnaire.Completed;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class NumericQuestionView : AbstractQuestionView
    {
       /* public NumericQuestionView(Context context, QuestionViewModel model) : base(context, model)
        {
        }*/

        public NumericQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source, Guid questionnairePublicKey)
            : base(context, bindingActivity, source, questionnairePublicKey)
        {
        }

       /* public NumericQuestionView(Context context, IAttributeSet attrs, int defStyle, QuestionViewModel model) : base(context, attrs, defStyle, model)
        {
        }

        public NumericQuestionView(IntPtr javaReference, JniHandleOwnership transfer, QuestionViewModel model) : base(javaReference, transfer, model)
        {
        }*/

        protected override void Initialize()
        {
            base.Initialize();
            llWrapper.Click += NumericQuestionView_Click;
            etAnswer=new EditText(this.Context);
            etAnswer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            etAnswer.Text = Model.AnswerString;
            etAnswer.InputType = InputTypes.ClassNumber;
            
            etAnswer.SetSelectAllOnFocus(true);
            etAnswer.ImeOptions=ImeAction.Done;
            etAnswer.SetSingleLine(true);
            etAnswer.EditorAction += etAnswer_EditorAction;
            etAnswer.FocusChange += etAnswer_FocusChange;
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
           /* InputMethodManager imm
               = (InputMethodManager)this.Context.GetSystemService(
                   Context.InputMethodService);
            if(imm.IsAcceptingText)
            {
                
            }*/
        }
        protected override void SaveAnswer()
        {
            try
            {
                tvError.Visibility = ViewStates.Gone;
                string newValue = etAnswer.Text.Trim();
                if (newValue != this.Model.AnswerString)
                {
                    CommandService.Execute(new SetAnswerCommand(this.QuestionnairePublicKey, Model.PublicKey.PublicKey,
                                                           null, newValue,
                                                           Model.PublicKey.PropagationKey));
                }
                base.SaveAnswer();
              
            }
            catch (Exception ex)
            {
                // etAnswer.Text = Model.AnswerString;
                tvError.Visibility = ViewStates.Visible;
                etAnswer.Text = Model.AnswerString;
                tvError.Text = (ex.InnerException ?? ex).Message;
            }
        }

        void etAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            etAnswer.ClearFocus();
        }
        void NumericQuestionView_Click(object sender, EventArgs e)
        {
            etAnswer.RequestFocus();
        }

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }

        protected EditText etAnswer { get; set; }
    }
}