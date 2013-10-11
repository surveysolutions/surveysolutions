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

    public class NumericIntegerQuestionView : NumericQuestionView
    {
        public NumericIntegerQuestionView(Context context, IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
            KeyboardTypeFlags = InputTypes.ClassNumber | InputTypes.NumberFlagSigned;
        }

        protected override bool IsParsingOrAswerSavingFailed(string newAnswer)
        {
            int answer;
            if (!int.TryParse(newAnswer, out answer))
                return true;

            ExecuteSaveAnswerCommand(new AnswerNumericIntegerQuestionCommand(this.QuestionnairePublicKey,
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id,
                this.Model.PublicKey.PropagationVector,
                DateTime.UtcNow, answer));

            return false;
        }
    }
    public class NumericRealQuestionView : NumericQuestionView
    {
        public NumericRealQuestionView(Context context, IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
            KeyboardTypeFlags = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal | InputTypes.NumberFlagSigned;
        }

        protected override bool IsParsingOrAswerSavingFailed(string newAnswer)
        {
            decimal answer;
            if (!decimal.TryParse(newAnswer, out answer))
                return true;

            ExecuteSaveAnswerCommand(new AnswerNumericRealQuestionCommand(this.QuestionnairePublicKey,
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id,
                this.Model.PublicKey.PropagationVector,
                DateTime.UtcNow, answer));

            return false;
        }
    }
    public abstract class NumericQuestionView : AbstractQuestionView
    {
        protected InputTypes KeyboardTypeFlags; 

        public NumericQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            llWrapper.Click += NumericQuestionView_Click;
            etAnswer=new EditText(this.Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent),
                Text = Model.AnswerString,
                InputType = KeyboardTypeFlags
            };

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
                return;
            }

            SaveAnswer(etAnswer.Text.Trim());
        }

        protected override void SaveAnswer(string newAnswer)
        {
            if (newAnswer != this.Model.AnswerString)
            {
                if (IsParsingOrAswerSavingFailed(newAnswer)) 
                    return;
                
                if (!IsCommentsEditorFocused)
                    HideKeyboard(etAnswer);

                base.SaveAnswer(newAnswer);
            }
        }

        protected abstract bool IsParsingOrAswerSavingFailed(string newAnswer);

        protected override void SaveAnswerErrorHandler(Exception ex)
        {
            base.SaveAnswerErrorHandler(ex);
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