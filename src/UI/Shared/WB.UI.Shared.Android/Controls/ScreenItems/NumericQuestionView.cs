using System;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems{
    public abstract class NumericQuestionView<T> : AbstractQuestionView
        where T : struct
    {
        protected abstract InputTypes KeyboardTypeFlags
        {
            get;
        }

        public NumericQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.llWrapper.Click += this.NumericQuestionView_Click;
            this.etAnswer = new EditText(this.Context);
            this.etAnswer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            this.etAnswer.InputType = this.KeyboardTypeFlags;

            this.PutAnswerStoredInModelToUI();

            this.etAnswer.SetSelectAllOnFocus(true);
            this.etAnswer.ImeOptions = ImeAction.Done;
            this.etAnswer.SetSingleLine(true);
            this.etAnswer.EditorAction += this.etAnswer_EditorAction;
            this.etAnswer.FocusChange += this.etAnswer_FocusChange;
            this.llWrapper.AddView(this.etAnswer);
        }

        void etAnswer_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                return;
            }

            string newAnswer = this.etAnswer.Text.Trim();

            T answer;
            if (!this.IsParseAnswerStringSucceeded(newAnswer, out answer))
                return;

            if (this.Model.AnswerObject!=null && answer.Equals(this.Model.AnswerObject)) return;

            if (!this.IsCommentsEditorFocused)
                this.HideKeyboard(this.etAnswer);

            this.SaveAnswer(newAnswer, this.CreateAnswerQuestionCommand(answer));
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
            this.etAnswer.ClearFocus();
        }
        void NumericQuestionView_Click(object sender, EventArgs e)
        {
            this.etAnswer.RequestFocus();
            this.ShowKeyboard(this.etAnswer);
        }

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }

        protected EditText etAnswer { get; set; }
    }
}