using System;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.Shared.Android.Controls.MaskedEditTextControl;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class TextQuestionView : AbstractQuestionView
    {
        public TextQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source,
                                Guid questionnairePublicKey, ICommandService commandService,
                                IAnswerOnQuestionCommandService answerCommandService, IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }
        protected ValueQuestionViewModel TypedMode
        {
            get { return this.Model as ValueQuestionViewModel; }
        }

        protected MaskedWatcher maskedWatcher;

        protected bool isInputMasked = false;

        protected override void Initialize()
        {
            base.Initialize();

            this.etAnswer = new EditText(this.CurrentContext);

            isInputMasked = !string.IsNullOrWhiteSpace(TypedMode.Mask);
            
            if (isInputMasked)
            {
                maskedWatcher = new MaskedWatcher(TypedMode.Mask, etAnswer);
                etAnswer.AddTextChangedListener(maskedWatcher);
                this.etAnswer.InputType = InputTypes.TextVariationVisiblePassword; //fix for samsung 
            }
            
            this.etAnswer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            
            this.etAnswer.SetSelectAllOnFocus(true);
            this.etAnswer.ImeOptions = ImeAction.Done;
            this.etAnswer.SetSingleLine(true);

            this.etAnswer.EditorAction += this.etAnswer_EditorAction;
            this.etAnswer.FocusChange += this.etAnswer_FocusChange;
            this.PutAnswerStoredInModelToUI();
            
            this.llWrapper.Click += this.TextQuestionView_Click;
            this.llWrapper.AddView(this.etAnswer);
        }

        private void etAnswer_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                return;
            }

            var newAnswer = etAnswer.Text.Trim();

            if (isInputMasked && !this.maskedWatcher.IsTextMaskMatched())
            {

                if (newAnswer != this.Model.AnswerString)
                {
                    if (Build.VERSION.SdkInt <= BuildVersionCodes.Kitkat) //temp fix Android Lollipop issue with scroll
                        this.PutAnswerStoredInModelToUI();
                }

                return;
            }

            if (newAnswer != this.Model.AnswerString)
            {
                this.SaveAnswer(newAnswer,
                    new AnswerTextQuestionCommand(
                        this.QuestionnairePublicKey, this.Membership.CurrentUser.Id, this.Model.PublicKey.Id,
                        this.Model.PublicKey.InterviewItemPropagationVector, DateTime.UtcNow, newAnswer));
            }
            else if (this.etComments.Visibility != ViewStates.Visible)
            {
                base.FireAnswerSavedEvent();
            }
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.etAnswer.Text = this.GetAnswerStoredInModelAsString();
        }

        private void etAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Done)
            {
                this.etAnswer.ClearFocus();
                this.HideKeyboard(this.etAnswer);
            }
        }

        private void TextQuestionView_Click(object sender, EventArgs e)
        {
            this.etAnswer.RequestFocus();
        }

        protected EditText etAnswer { get; set; }
    }
}