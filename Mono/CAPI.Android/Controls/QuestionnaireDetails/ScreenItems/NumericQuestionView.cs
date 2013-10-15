using System;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public abstract class NumericQuestionView : AbstractQuestionView
    {
        protected abstract InputTypes KeyboardTypeFlags
        {
             get;
        }

        public NumericQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            llWrapper.Click += NumericQuestionView_Click;
            etAnswer = new EditText(this.Context);
            etAnswer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            etAnswer.InputType = KeyboardTypeFlags;

            this.PutAnswerStoredInModelToUI();

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

            string newAnswer = this.etAnswer.Text.Trim();

            if (newAnswer != this.Model.AnswerString)
            {
                ParseAndSaveAnswer(newAnswer);
            }
        }

        protected abstract void ParseAndSaveAnswer(string newAnswer);

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.etAnswer.Text = this.GetAnswerStoredInModelAsString();
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