using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public abstract class AbstractMultyQuestionView<T>: AbstractQuestionView where T: class 
    {
        protected LinearLayout AnswersCheckboxContainer;
        protected abstract IEnumerable<T> Answers { get; }
        protected abstract int? MaxAllowedAnswers { get; }
        protected abstract bool? IsAnswersOrdered { get; }

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
            answeredCount = 0;
            this.AnswersCheckboxContainer = this.CreateCheckBoxes();

            this.PutAnswerStoredInModelToUI();

            llWrapper.AddView(this.AnswersCheckboxContainer);
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            IEnumerable<T> selectedAnswers = this.Answers.Where(this.IsAnswerSelected);

            return this.FormatSelectedAnswersAsString(selectedAnswers);
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.CreateCheckBoxesByOptions();
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
            this.AnswersCheckboxContainer.RemoveAllViews();

            foreach (var answer in this.Answers)
            {
                var answerBlock = this.CreateAnswerBlock(answer);

                this.AnswersCheckboxContainer.AddView(answerBlock);
            }
        }

        protected abstract string GetAnswerId(T answer);

        protected abstract string GetAnswerTitle(T answer);

        protected abstract T FindAnswerInModelByCheckBoxTag(string tag);

        protected abstract AnswerQuestionCommand CreateSaveAnswerCommand(T[] selectedAnswers);

        protected abstract bool IsAnswerSelected(T answer);

        protected abstract int GetAnswerOrder(T answer);

        protected virtual void AddAdditionalAttributes(CheckBox checkBox, T answer) { }

        private RelativeLayout CreateAnswerBlock(T answer)
        {
            var container = new RelativeLayout(this.Context);
            container.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                              ViewGroup.LayoutParams.FillParent);

            CheckBox cb = new CheckBox(this.Context);
            var cbLayoutParams = new RelativeLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);

            cbLayoutParams.AddRule(LayoutRules.AlignParentLeft);
            cbLayoutParams.SetMargins(20,0,0,0);
            cb.LayoutParameters = cbLayoutParams;
            cb.Checked = this.IsAnswerSelected(answer);
            
            if (cb.Checked)
                answeredCount++;

            cb.Text = this.GetAnswerTitle(answer);

            if (this.MaxAllowedAnswers.HasValue)
                cb.Click += (sender, args) => { 
                    CheckBox checkBox = (CheckBox)sender;
                                              if (checkBox.Checked)
                                              {
                                                  if (this.MaxAllowedAnswers.HasValue && answeredCount >= this.MaxAllowedAnswers)
                                                  {
                                                      return;

                                                  }
                                              }
                };

            cb.CheckedChange += RadioGroupCheckedChange;
            cb.SetTag(Resource.Id.AnswerId, GetAnswerId(answer));
            AddAdditionalAttributes(cb, answer);

            if (this.IsAnswersOrdered == true)
            {
                int AnswerOrder = GetAnswerOrder(answer);
                TextView AnswerOrderText = new TextView(this.Context);
                AnswerOrderText.SetTypeface(null, TypefaceStyle.Bold);
                
                var layoutParams = new RelativeLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
                layoutParams.AddRule(LayoutRules.AlignParentLeft);
                AnswerOrderText.LayoutParameters = layoutParams;

                if (AnswerOrder > 0 && cb.Checked)
                {
                    AnswerOrderText.Text = AnswerOrder.ToString(CultureInfo.InvariantCulture);
                    AnswerOrderText.SetBackgroundColor(Color.LightBlue);
                }
                container.AddView(AnswerOrderText);
            }
            container.AddView(cb);
            return container;
        }

        private T GetFirstChildTypeOf<T>(RelativeLayout layout) where T:class
        {
            if (layout == null)
                return null;
            for (int i = 0; i < layout.ChildCount; i++)
            {
                var child = layout.GetChildAt(i) as T;
                if (child != null)
                    return child; 
            }
            return null;
        }
        
        void RadioGroupCheckedChange(object sender, CheckBox.CheckedChangeEventArgs e)
        {
            var selectedAnswers = new List<T>();
            for (int i = 0; i < AnswersCheckboxContainer.ChildCount; i++)
            {
                var itemContainer = AnswersCheckboxContainer.GetChildAt(i) as RelativeLayout;
                var checkBox = GetFirstChildTypeOf<CheckBox>(itemContainer);
                if(checkBox==null)
                    continue;

                if(checkBox.Checked)
                    selectedAnswers.Add(this.FindAnswerInModelByCheckBoxTag(checkBox.GetTag(Resource.Id.AnswerId).ToString()));
            }

            if (this.MaxAllowedAnswers.HasValue && selectedAnswers.Count > this.MaxAllowedAnswers)
            {
                return;
            }

            this.SaveAnswer(this.FormatSelectedAnswersAsString(selectedAnswers), CreateSaveAnswerCommand(selectedAnswers.ToArray()));

            answeredCount = selectedAnswers.Count;
        }

        private string FormatSelectedAnswersAsString(IEnumerable<T> selectedAnswers)
        {
            return string.Join(",", selectedAnswers.Select(this.GetAnswerTitle));
        }

        private int answeredCount;
    }
}