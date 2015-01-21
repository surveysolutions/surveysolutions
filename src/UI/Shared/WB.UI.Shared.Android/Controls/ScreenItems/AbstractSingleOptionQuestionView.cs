using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public abstract class AbstractSingleOptionQuestionView<T> : AbstractQuestionView where T: class 
    {
        protected RadioGroup RadioGroup;
        protected abstract IEnumerable<T> Answers { get; }

        protected AbstractSingleOptionQuestionView(
            Context context, 
            IMvxAndroidBindingContext bindingActivity, 
            QuestionViewModel source,
            Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            this.RadioGroup = this.CreateRadioButtonsGroup();
            this.PutAnswerStoredInModelToUI();

            this.llWrapper.AddView(this.RadioGroup);
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            T selectedAnswer = this.Answers.SingleOrDefault(this.IsAnswerSelected);

            return selectedAnswer != null
                ? this.GetAnswerTitle(selectedAnswer)
                : string.Empty;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.FillRadioButtonGroupWithAnswers();
        }

        protected RadioGroup CreateRadioButtonsGroup()
        {
            var radioGroup = new RadioGroup(this.CurrentContext);
            radioGroup.Orientation = Orientation.Vertical;
            radioGroup.CheckedChange += this.RadioGroupCheckedChange;
            return radioGroup;
        }

        protected void FillRadioButtonGroupWithAnswers()
        {
            this.RadioGroup.CheckedChange -= this.RadioGroupCheckedChange;

            try
            {
                this.RadioGroup.RemoveAllViews();

                RadioButton checkedButton = null;
                int i = 0;

                foreach (var answer in this.Answers)
                {
                    var radioButton = this.CreateRadioButton(answer);

                    this.RadioGroup.AddView(radioButton);

                    if (this.IsAnswerSelected(answer))
                        checkedButton = radioButton;

                    i++;
                }

                if (checkedButton != null)
                {
                    this.RadioGroup.Check(checkedButton.Id);
                }
            }
            finally
            {
                this.RadioGroup.CheckedChange += this.RadioGroupCheckedChange;
            }
        }

        protected abstract string GetAnswerId(T answer);

        protected abstract string GetAnswerTitle(T answer);

        protected abstract T FindAnswerInModelByRadioButtonTag(string tag);

        protected abstract AnswerQuestionCommand CreateSaveAnswerCommand(T selectedAnswer);

        protected abstract bool IsAnswerSelected(T answer);

        protected virtual void AddAdditionalAttributes(RadioButton radioButton, T answer) {}

        private RadioButton CreateRadioButton(T answer)
        {
            var radioButton = new RadioButton(this.CurrentContext);
            radioButton.Text = this.GetAnswerTitle(answer);
            radioButton.SetTag(Resource.Id.AnswerId, this.GetAnswerId(answer));
            this.AddAdditionalAttributes(radioButton,answer);
            return radioButton;
        }

        void RadioGroupCheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            var selectedItem = this.RadioGroup.FindViewById<RadioButton>(e.CheckedId);

            T selectedAnswer = this.FindAnswerInModelByRadioButtonTag(selectedItem.GetTag(Resource.Id.AnswerId).ToString());

            if (selectedAnswer == null)
                return;

            this.SaveAnswer(this.GetAnswerTitle(selectedAnswer), this.CreateSaveAnswerCommand(selectedAnswer));
        }
    }
}