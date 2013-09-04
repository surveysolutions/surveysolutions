using System;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Main.Core.Commands.Questionnaire.Completed;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class NumericQuestionView : AbstractQuestionView
    {

        public NumericQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            llWrapper.Click += NumericQuestionView_Click;
            etAnswer=new EditText(this.Context);
            etAnswer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            etAnswer.Text = Model.AnswerString;
            etAnswer.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;
            
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
                //ShowKeyboard(etAnswer);
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
                    ExecuteSaveAnswerCommand(new AnswerNumericQuestionCommand(this.QuestionnairePublicKey,
                                                                            CapiApplication.Membership.CurrentUser.Id,
                                                                            Model.PublicKey.Id,
                                                                            this.Model.PublicKey.PropagationVector, DateTime.UtcNow, decimal.Parse(newValue)));
                if (!IsCommentsEditorFocused)
                    HideKeyboard(etAnswer);
            }
            base.SaveAnswer();
        }

        protected override void SaveAnswerErrorHappend()
        {
            etAnswer.Text = Model.AnswerString;
        }

        void etAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            etAnswer.ClearFocus();
        }
        void NumericQuestionView_Click(object sender, EventArgs e)
        {
            etAnswer.RequestFocus();
            ShowKeyboard(etAnswer);
        }

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }

        protected EditText etAnswer { get; set; }
    }
}