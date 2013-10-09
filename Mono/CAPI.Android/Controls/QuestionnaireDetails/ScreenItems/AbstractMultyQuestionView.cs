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
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public abstract class AbstractMultyQuestionView<T>: AbstractQuestionView where T: class 
    {
        protected LinearLayout CheckBoxContainer;
        protected abstract IEnumerable<T> Answers { get; }

        protected AbstractMultyQuestionView(
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
            this.Orientation = Orientation.Vertical;

            this.CheckBoxContainer = this.CreateCheckBoxes();

            this.CreateCheckBoxesByOptions();

            llWrapper.AddView(this.CheckBoxContainer);
        }

        protected LinearLayout CreateCheckBoxes()
        {
            var optionsWrapper = new LinearLayout(this.Context);
            optionsWrapper.Orientation = Orientation.Vertical;
            optionsWrapper.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            return optionsWrapper;
        }

        protected void CreateCheckBoxesByOptions()
        {
            this.CheckBoxContainer.RemoveAllViews();

            foreach (var answer in this.Answers)
            {
                var checkBox = this.CreateCheckBox(answer);

                this.CheckBoxContainer.AddView(checkBox);
            }
        }

        protected abstract string GetAnswerId(T answer);

        protected abstract string GetAnswerTitle(T answer);

        protected abstract T FindAnswerInModelByCheckBoxTag(string tag);

        protected abstract AnswerQuestionCommand CreateSaveAnswerCommand(T[] selectedAnswers);

        protected abstract bool IsAnswerSelected(T answer);

        protected virtual void AddAdditionalAttributes(CheckBox checkBox, T answer) { }

        private CheckBox CreateCheckBox(T answer)
        {
            CheckBox cb = new CheckBox(this.Context);
            cb.Text = this.GetAnswerTitle(answer);
            cb.Checked = this.IsAnswerSelected(answer);
            cb.CheckedChange += RadioGroupCheckedChange;
            cb.SetTag(Resource.Id.AnswerId, GetAnswerId(answer));
            AddAdditionalAttributes(cb,answer);
            return cb;
        }

        void RadioGroupCheckedChange(object sender, CheckBox.CheckedChangeEventArgs e)
        {
            var selectedAnswers = new List<T>();
            for (int i = 0; i < CheckBoxContainer.ChildCount; i++)
            {
                var checkBox = CheckBoxContainer.GetChildAt(i) as CheckBox;
                if(checkBox==null)
                    continue;
                if(checkBox.Checked)
                    selectedAnswers.Add(this.FindAnswerInModelByCheckBoxTag(checkBox.GetTag(Resource.Id.AnswerId).ToString()));
                
            }

            this.SaveAnswer(string.Join(",", selectedAnswers.Select(this.GetAnswerTitle)), CreateSaveAnswerCommand(selectedAnswers.ToArray()));
        }
    }
}