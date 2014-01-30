using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public abstract class AbstractTextListQuestionView<T>: AbstractQuestionView where T: class
    {
        private const string AddListItemText = "+";
        private const string RemoveListItemText = "-";

        protected LinearLayout AnswersContainer;
        protected LinearLayout ActionsContainer;
        protected Button AddItemView;
        protected int ItemsCount;


        protected abstract IEnumerable<T> ListAnswers { get; }
        protected abstract int? MaxAnswerCount { get; }
        
        protected AbstractTextListQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, 
            QuestionViewModel source, Guid questionnairePublicKey, ICommandService commandService, 
            IAnswerOnQuestionCommandService answerCommandService, IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Orientation = Orientation.Vertical;
            this.AddItemView = CreateAddListButton();
            this.AnswersContainer = this.CreateContainer();
            this.ActionsContainer = this.CreateActionContainer(AddItemView);
            
            this.llWrapper.AddView(this.AnswersContainer);
            this.llWrapper.AddView(this.ActionsContainer);
            
            this.PutAnswerStoredInModelToUI();

            ItemsCount = this.ListAnswers.Count();
            if (IsMaxAnswerCountExceeded(ItemsCount))
                AddItemView.Enabled = false;
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.FormatSelectedAnswersAsString(this.ListAnswers);
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.AnswersContainer.RemoveAllViews();

            foreach (var answer in this.ListAnswers)
            {
                var answerBlock = this.CreateAnswerBlock(this.GetAnswerId(answer), this.GetAnswerTitle(answer));
                this.AnswersContainer.AddView(answerBlock);
            }
        }

        private Button CreateAddListButton()
        {
            Button button = new Button(this.Context);
            var cbLayoutParams = new RelativeLayout.LayoutParams(LayoutParams.WrapContent,
                                                                 LayoutParams.WrapContent);

            cbLayoutParams.AddRule(LayoutRules.AlignParentRight);
            button.LayoutParameters = cbLayoutParams;
            button.Text = AddListItemText;
            button.SetTypeface(null, TypefaceStyle.Bold);
            button.Click += this.AddListItemButtonClick;

            return button;
        }

        private void AddListItemButtonClick(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;
            
            //change value generation
            var listAnswers = this.GetAnswersFromUI();
            var maxValue = listAnswers.Count == 0 ? 0 : listAnswers.Max(i => i.Value);

            var newBlock = CreateAnswerBlock((maxValue + 1).ToString(), "");
            this.AnswersContainer.AddView(newBlock);

            var newEditor = GetFirstChildTypeOf<EditText>(newBlock);
            if (newEditor != null)
            {
                newEditor.RequestFocus();
                this.ShowKeyboard(newEditor);
            }

            ItemsCount++;
            if (IsMaxAnswerCountExceeded(ItemsCount))
                AddItemView.Enabled = false;
        }

        private void RemoveListItemClick(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;
            
            string buttonTag = button.GetTag(Resource.Id.AnswerId).ToString();
            decimal listItemValue;
            if (!decimal.TryParse(buttonTag, out listItemValue))
            {
                return; //ignore unknown tag
            }
            
            var item = FindAnswerInModelByTag(buttonTag);
            if (item != null)
            {
                var answersToSave = GetNewListAnswersToSave(buttonTag, "", false).ToArray();
                this.SaveAnswer(this.FormatSelectedAnswersAsString(answersToSave), this.CreateSaveAnswerCommand(answersToSave.ToArray()));
            }

            RemoveFirstChildByTag(this.AnswersContainer, buttonTag);
            ItemsCount--;

            if (!IsMaxAnswerCountExceeded(ItemsCount))
                AddItemView.Enabled = true;

        }

        private void RemoveFirstChildByTag(LinearLayout container, string itemTagToRevove)
        {
            for (int i = 0; i < container.ChildCount; i++)
            {
                var itemContainer = container.GetChildAt(i) as ViewGroup;
                if (itemContainer == null) 
                    continue;

                var tag = itemContainer.GetTag(Resource.Id.AnswerId).ToString();
                if (itemTagToRevove == tag)
                {
                    container.RemoveView(itemContainer);
                    return;
                }
            }
        }

        protected LinearLayout CreateContainer()
        {
            var optionsWrapper = new LinearLayout(this.Context);
            optionsWrapper.Orientation = Orientation.Vertical;
            optionsWrapper.LayoutParameters = new LayoutParams(LayoutParams.FillParent,
                                                               LayoutParams.FillParent);
            
            return optionsWrapper;
        }

        protected LinearLayout CreateActionContainer(Button addButton)
        {
            var optionsWrapper = CreateContainer();
            var container = new RelativeLayout(this.Context);
            container.LayoutParameters = new LayoutParams(LayoutParams.FillParent,
                                                          LayoutParams.FillParent);

            container.AddView(addButton);
            optionsWrapper.AddView(container);
            return optionsWrapper;
        }

        protected abstract string GetAnswerId(T answer);

        protected abstract string GetAnswerTitle(T answer);

        protected abstract AnswerQuestionCommand CreateSaveAnswerCommand(T[] selectedAnswers);

        private LinearLayout CreateAnswerBlock(string answerValueTag, string answerTitle)
        {
            var container = new LinearLayout(this.Context);
            container.LayoutParameters = new LayoutParams(LayoutParams.FillParent,
                                                          LayoutParams.FillParent);
            
            container.SetTag(Resource.Id.AnswerId, answerValueTag);

            var text = CreateValueEditor(answerValueTag, answerTitle);

            Button button = this.CreateRemoveListItemButton(answerValueTag);

            container.AddView(text);
            container.AddView(button);

            return container;
        }

        private EditText CreateValueEditor(string answerValueTag, string answerTitle)
        {
            EditText text = new EditText(this.Context);
            text.SetSelectAllOnFocus(true);
            text.ImeOptions = ImeAction.Done;
            text.SetSingleLine(true);
            text.FocusChange += this.textAnswer_FocusChange;
            text.EditorAction += textAnswer_EditorAction;

            var layoutParams = new LayoutParams(LayoutParams.FillParent,
                                                LayoutParams.WrapContent);

            layoutParams.Weight = 1.0f;
            text.LayoutParameters = layoutParams;

            text.Text = answerTitle;
            text.SetTag(Resource.Id.AnswerId, answerValueTag);
            return text;
        }

        private void textAnswer_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                return;
            }
            
            var editor = sender as EditText;
            if (editor == null)
                return;

            var newAnswer = editor.Text.Trim();
            if(String.IsNullOrWhiteSpace(newAnswer))
                return;
            
            var tagName = editor.GetTag(Resource.Id.AnswerId).ToString();

            if (!DoesModelHaveItem(tagName, newAnswer))
            {
                var answers = GetNewListAnswersToSave(tagName, newAnswer, true).ToArray();
                this.SaveAnswer(this.FormatSelectedAnswersAsString(answers), this.CreateSaveAnswerCommand(answers));
            }
        }

        protected abstract bool DoesModelHaveItem(string tagName, string value);
        protected abstract IEnumerable<T> GetNewListAnswersToSave(string tagName, string newAnswer, bool addOrRemove);
        

        private void textAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            var editor = sender as EditText;
            if(editor != null)
                editor.ClearFocus();
        }

        protected abstract T FindAnswerInModelByTag(string tag);

        private Button CreateRemoveListItemButton(string answerTag)
        {
            Button button = new Button(this.Context);

            var layoutParams = new LayoutParams(LayoutParams.WrapContent,
                                                LayoutParams.WrapContent);
            
            button.LayoutParameters = layoutParams;

            button.SetTypeface(null, TypefaceStyle.Bold);
            button.Text = RemoveListItemText;
            
            button.Click += this.RemoveListItemClick;
            button.SetTag(Resource.Id.AnswerId, answerTag);
            
            return button;
        }

        private T GetFirstChildTypeOf<T>(ViewGroup layout) where T : class
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

        private bool IsMaxAnswerCountExceeded(int valueToCheck)
        {
            return this.MaxAnswerCount.HasValue && valueToCheck >= this.MaxAnswerCount;
        }

        private List<TextListAnswerViewModel> GetAnswersFromUI()
        {
            var answers = new List<TextListAnswerViewModel>();
            for (int i = 0; i < this.AnswersContainer.ChildCount; i++)
            {
                var itemContainer = this.AnswersContainer.GetChildAt(i) as ViewGroup;
                var editText = this.GetFirstChildTypeOf<EditText>(itemContainer);
                
                if (editText == null)
                    continue;

                var listItemValue =  decimal.Parse(editText.GetTag(Resource.Id.AnswerId).ToString());
               
                var item = new TextListAnswerViewModel(listItemValue.ToString(), editText.Text.Trim());

                answers.Add(item);
            }
            return answers;
        }

        private string FormatSelectedAnswersAsString(IEnumerable<T> selectedAnswers)
        {
            return string.Join(",", selectedAnswers.Select(this.GetAnswerTitle));
        }
    }
}