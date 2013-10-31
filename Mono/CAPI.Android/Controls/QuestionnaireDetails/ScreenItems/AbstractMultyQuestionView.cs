using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public abstract class AbstractMultyQuestionView<T>: AbstractQuestionView where T: class 
    {
        protected LinearLayout AnswersContainer;
        protected abstract IEnumerable<T> Answers { get; }
        protected abstract int? MaxAllowedAnswers { get; }
        protected abstract bool? AreAnswersOrdered { get; }

        private Dictionary<string, int> givenAnswersWithOrder;
        
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
            this.AnswersContainer = this.CreateAnswersContainer();
            this.givenAnswersWithOrder = new Dictionary<string, int>();

            this.PutAnswerStoredInModelToUI();

            llWrapper.AddView(this.AnswersContainer);
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            IEnumerable<T> selectedAnswers = this.Answers.Where(this.IsAnswerSelected);

            return this.FormatSelectedAnswersAsString(selectedAnswers);
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.CreateAnswersByOptions();
        }

        protected LinearLayout CreateAnswersContainer()
        {
            var optionsWrapper = new LinearLayout(this.Context);
            optionsWrapper.Orientation = Orientation.Vertical;
            optionsWrapper.LayoutParameters = new LayoutParams(LayoutParams.FillParent,
                                                               LayoutParams.FillParent);
            return optionsWrapper;
        }
        
        protected void CreateAnswersByOptions()
        {
            this.AnswersContainer.RemoveAllViews();
            givenAnswersWithOrder.Clear();

            foreach (var answer in this.Answers)
            {
                var answerBlock = this.CreateAnswerBlock(answer);

                this.AnswersContainer.AddView(answerBlock);
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
            container.LayoutParameters = new LayoutParams(LayoutParams.FillParent,
                                                          LayoutParams.FillParent);
            
            string answerTagId = GetAnswerId(answer);
            CheckBox cb = CreateCheckBox(answer, answerTagId);
            
            if (this.AreAnswersOrdered == true || this.MaxAllowedAnswers.HasValue)
            {
                int answerOrder = GetAnswerOrder(answer);
                    
                if (cb.Checked)
                    givenAnswersWithOrder.Add(answerTagId, answerOrder);

                if (this.AreAnswersOrdered == true)
                {
                    var answerOrderText = CreateOrderText(answerOrder);
                    container.AddView(answerOrderText);
                }
            }

            container.AddView(cb);
            return container;
        }

        private TextView CreateOrderText(int answerOrder)
        {
            TextView answerOrderText = new TextView(this.Context);
            answerOrderText.SetTypeface(null, TypefaceStyle.Bold);

            var layoutParams = new RelativeLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
            layoutParams.AddRule(LayoutRules.AlignParentLeft);
            answerOrderText.LayoutParameters = layoutParams;
            
            if (answerOrder > 0)
            {
                answerOrderText.Text = answerOrder.ToString(CultureInfo.InvariantCulture);
            }
            
            return answerOrderText;
        }

        private CheckBox CreateCheckBox(T answer, string answerTag)
        {
            CheckBox cb = new CheckBox(this.Context);
            
            var cbLayoutParams = new RelativeLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
            cbLayoutParams.AddRule(LayoutRules.AlignParentLeft);
            cbLayoutParams.SetMargins(20, 0, 0, 0);
            cb.LayoutParameters = cbLayoutParams;

            cb.Checked = this.IsAnswerSelected(answer);
            cb.Text = this.GetAnswerTitle(answer);

            cb.CheckedChange += CheckBoxCheckedChange;
            cb.SetTag(Resource.Id.AnswerId, answerTag);
            AddAdditionalAttributes(cb, answer);

            return cb;
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

        private void UpdateAnswerOrderList(string changedAnswerTag, bool isChecked)
        {
            if (!isChecked)
            {
                if (givenAnswersWithOrder.ContainsKey(changedAnswerTag))
                {
                    int order = givenAnswersWithOrder[changedAnswerTag];
                    givenAnswersWithOrder = givenAnswersWithOrder.Where(answer => answer.Key != changedAnswerTag)
                                                             .ToDictionary(answer => answer.Key,
                                                                           answer =>
                                                                           answer.Value > order
                                                                               ? answer.Value - 1
                                                                               : answer.Value);
                }
            }
            else
            {
                if (!givenAnswersWithOrder.ContainsKey(changedAnswerTag))
                {
                    givenAnswersWithOrder.Add(changedAnswerTag, givenAnswersWithOrder.Count() + 1);
                }
            }
        }

        private bool MaxAllowedAnswersCountExceeded()
        {
            return this.MaxAllowedAnswers.HasValue && orderedGivenAnswers.Count() >= this.MaxAllowedAnswers;
        }

        private bool IsUpdateAnswerOrderListNeeded()
        {
            return this.AreAnswersOrdered == true || this.MaxAllowedAnswers.HasValue;
        }

        private void CheckBoxCheckedChange(object sender, CheckBox.CheckedChangeEventArgs e)
        {
            var checkedBox = sender as CheckBox;
            if (checkedBox == null)
                return;

            if (e.IsChecked && MaxAllowedAnswersCountExceeded())
            {
                checkedBox.Checked = false;
                return;
            }
           
            if (IsUpdateAnswerOrderListNeeded())
            {
                string changedAnswerTag = checkedBox.GetTag(Resource.Id.AnswerId).ToString();
                UpdateAnswerOrderList(changedAnswerTag, e.IsChecked);
            }

            var selectedAnswers = this.GetSelectedAnswers();

            this.SaveAnswer(this.FormatSelectedAnswersAsString(selectedAnswers), CreateSaveAnswerCommand(selectedAnswers.ToArray()));
        }

        private List<T> GetSelectedAnswers()
        {
            var selectedAnswers = new List<T>();
            for (int i = 0; i < this.AnswersContainer.ChildCount; i++)
            {
                var itemContainer = this.AnswersContainer.GetChildAt(i) as RelativeLayout;
                var checkBox = this.GetFirstChildTypeOf<CheckBox>(itemContainer);
                if (checkBox == null)
                    continue;

                string answerTag = checkBox.GetTag(Resource.Id.AnswerId).ToString();

                if (this.AreAnswersOrdered == true)
                {
                    var answerOrderText = this.GetFirstChildTypeOf<TextView>(checkBox.Parent as RelativeLayout);
                    if (answerOrderText != null)
                        answerOrderText.Text = this.givenAnswersWithOrder.ContainsKey(answerTag) ?
                            givenAnswersWithOrder[answerTag].ToString(CultureInfo.InvariantCulture) :
                            : "";
                }

                if (checkBox.Checked)
                    selectedAnswers.Add(this.FindAnswerInModelByCheckBoxTag(answerTag));
            }
            return selectedAnswers;
        }

        private string FormatSelectedAnswersAsString(IEnumerable<T> selectedAnswers)
        {
            return string.Join(",", selectedAnswers.Select(this.GetAnswerTitle));
        }
    }
}