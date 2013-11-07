using System;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public abstract class NumericQuestionView<T> : AbstractQuestionView
        where T : struct
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
            etAnswer.ImeOptions = ImeAction.Done;
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

            T answer;
            if (!IsParseAnswerStringSucceeded(newAnswer, out answer))
                return;

            if (this.Model.AnswerObject!=null && answer.Equals(this.Model.AnswerObject)) return;

            if (!IsCommentsEditorFocused)
                HideKeyboard(etAnswer);

            this.SaveAnswer(newAnswer, CreateAnswerQuestionCommand(answer));
        }

        protected abstract bool IsParseAnswerStringSucceeded(string newAnswer, out T answer);

        protected abstract AnswerQuestionCommand CreateAnswerQuestionCommand(T answer);

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