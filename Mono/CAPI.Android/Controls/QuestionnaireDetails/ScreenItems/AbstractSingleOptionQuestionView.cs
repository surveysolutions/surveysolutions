using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
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
            IAnswerOnQuestionCommandService commandService)

            : base(context, bindingActivity, source, questionnairePublicKey, commandService)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            RadioGroup = this.CreateRadioButtonsGroup();
            this.PutAnswerStoredInModelToUI();
            RadioGroup.CheckedChange += this.RadioGroupCheckedChange;

            llWrapper.AddView(this.RadioGroup);
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.FillRadioButtonGroupWithAnswers();
        }

        protected RadioGroup CreateRadioButtonsGroup()
        {
            var radioGroup = new RadioGroup(this.Context);
            radioGroup.Orientation = Orientation.Vertical;
            return radioGroup;
        }

        protected void FillRadioButtonGroupWithAnswers()
        {
            RadioGroup.RemoveAllViews();

            RadioButton checkedButton = null;
            int i = 0;

            foreach (var answer in this.Answers)
            {
                var radioButton = this.CreateRadioButton(answer);

                RadioGroup.AddView(radioButton);

                if (this.IsAnswerSelected(answer))
                    checkedButton = radioButton;

                i++;
            }

            if (checkedButton != null)
            {
                RadioGroup.Check(checkedButton.Id);
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
            var radioButton = new RadioButton(this.Context);
            radioButton.Text = this.GetAnswerTitle(answer);
            radioButton.SetTag(Resource.Id.AnswerId, GetAnswerId(answer));
            AddAdditionalAttributes(radioButton,answer);
            return radioButton;
        }

        void RadioGroupCheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            var selectedItem = this.RadioGroup.FindViewById<RadioButton>(e.CheckedId);

            var selectedAnswer = FindAnswerInModelByRadioButtonTag(selectedItem.GetTag(Resource.Id.AnswerId).ToString());

            if (selectedAnswer == null)
                return;

            this.SaveAnswer(this.GetAnswerTitle(selectedAnswer), CreateSaveAnswerCommand(selectedAnswer));
        }
    }
}