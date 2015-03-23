using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Microsoft.Practices.ServiceLocation;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems{
    public abstract class NumericQuestionView<T> : AbstractQuestionView
        where T : struct
    {
        protected abstract InputTypes KeyboardTypeFlags
        {
            get;
        }

        protected ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        protected abstract string FormatString(string s);

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
            this.etAnswer = new EditText(this.CurrentContext);
            this.etAnswer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            this.etAnswer.InputType = this.KeyboardTypeFlags;

            this.etAnswer.SetSelectAllOnFocus(true);
            this.etAnswer.ImeOptions = ImeAction.Done;
            this.etAnswer.SetSingleLine(true);
            this.etAnswer.EditorAction += this.etAnswer_EditorAction;
            this.etAnswer.FocusChange += this.etAnswer_FocusChange;
            this.etAnswer.AfterTextChanged += etAnswer_AfterTextChanged;

            this.PutAnswerStoredInModelToUI();
            this.llWrapper.AddView(this.etAnswer);
        }

        private void etAnswer_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            this.etAnswer.AfterTextChanged -= etAnswer_AfterTextChanged;
            try
            {
                var newValue = FormatString(etAnswer.Text);
                if (newValue != etAnswer.Text)
                {
                    var newCursorPosition = GetNewCursorPosition(etAnswer.Text, newValue, etAnswer.SelectionEnd);
                    etAnswer.Text = newValue;
                    etAnswer.SetSelection(newCursorPosition);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception during numeric question's answer formatting", ex);
            }
            finally
            {
                this.etAnswer.AfterTextChanged += etAnswer_AfterTextChanged;
            }
        }

        private int GetNewCursorPosition(string oldText, string newText, int oldCursorPosition)
        {
            var newCursorPosition = newText.Length;
            var indexOfOldValue = 0;
            
            for (int i = 0; i < newText.Length; i++)
            {
                while (newText[i] != oldText[indexOfOldValue])
                {
                    if (!Char.IsNumber(newText[i]))
                        break;

                    indexOfOldValue++;
                }

                if (indexOfOldValue + 1 >= oldCursorPosition)
                {
                    newCursorPosition = i + 1;
                    break;
                }
            }

            return newCursorPosition;
        }

        async void etAnswer_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                return;
            }

            string newAnswer = this.etAnswer.Text.Trim();

            T answer;
            try
            {
                if (!this.IsParseAnswerStringSucceeded(newAnswer, out answer))
                    return;
            }
            catch (Exception ex)
            {
                Logger.Error("Exception during numeric question's answer parsing", ex);
                return;
            }

            if (this.Model.AnswerObject != null && answer.Equals(this.Model.AnswerObject))
            {
                if (this.etComments.Visibility != ViewStates.Visible)
                {
                    base.FireAnswerSavedEvent();
                }
                return;
            }
            var command = await this.CreateAnswerQuestionCommand(answer);
            if (command == null)
            {
                this.PutAnswerStoredInModelToUI();
                return;
            }
            this.SaveAnswer(newAnswer, command);
        }

        protected abstract bool IsParseAnswerStringSucceeded(string newAnswer, out T answer);

        protected abstract  Task<AnswerQuestionCommand> CreateAnswerQuestionCommand(T answer);

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
            if (e.ActionId == ImeAction.Done)
            {
                this.etAnswer.ClearFocus();
                this.HideKeyboard(this.etAnswer);
            }
            
        }
        void NumericQuestionView_Click(object sender, EventArgs e)
        {
            this.etAnswer.RequestFocus();
        }

        protected TextView tvTitle
        {
            get { return this.FindViewById<TextView>(Resource.Id.tvTitle); }
        }

        protected EditText etAnswer { get; set; }
    }
}