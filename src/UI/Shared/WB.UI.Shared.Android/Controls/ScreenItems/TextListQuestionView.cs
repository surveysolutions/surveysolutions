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
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class TextListQuestionView :  AbstractQuestionView
    {
        protected LinearLayout AnswersContainer;
        protected LinearLayout ActionsContainer;
        protected Button AddItemView;
        protected int ItemsCount;
        
        private const string AddListItemText = "+";
        private const string RemoveListItemText = "-";

        public TextListQuestionView(Context context, IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source, Guid questionnairePublicKey,
            ICommandService commandService, IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(
                context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService,
                membership)
        {
        }

        protected IEnumerable<TextListAnswerViewModel> ListAnswers
        {
            get { return TypedModel.ListAnswers; }
        }

        protected TextListQuestionViewModel TypedModel
        {
            get { return Model as TextListQuestionViewModel; }
        }

        protected int? MaxAnswerCount
        {
            get { return TypedModel.MaxAnswerCount; }
        }

        protected string GetAnswerId(TextListAnswerViewModel answer)
        {
            return answer.Value.ToString();
        }

        protected string GetAnswerTitle(TextListAnswerViewModel answer)
        {
            return answer.Answer;
        }

        protected AnswerQuestionCommand CreateSaveAnswerCommand(TextListAnswerViewModel[] selectedAnswers)
        {
            List<Tuple<decimal, string>> answers =
                selectedAnswers.Select(a => new Tuple<decimal, string>(a.Value, a.Answer)).ToList();

            return new AnswerTextListQuestionCommand(QuestionnairePublicKey, Membership.CurrentUser.Id,
                Model.PublicKey.Id, Model.PublicKey.InterviewItemPropagationVector, DateTime.UtcNow, answers.ToArray());
        }

        protected bool DoesModelHaveItem(string tagName, string newAnswer)
        {
            decimal value = decimal.Parse(tagName);
            return ListAnswers.Any(i => i.Value == value && i.Answer == newAnswer);
        }

        /*protected IEnumerable<TextListAnswerViewModel> GetNewListAnswersToSave(string tagName, string newAnswer,
            bool addOrRemove)
        {
            decimal value = decimal.Parse(tagName);

            if (!addOrRemove)
                return ListAnswers.Where(i => i.Value != value).ToList();

            List<TextListAnswerViewModel> newListAnswers = ListAnswers.ToList();
            TextListAnswerViewModel item = ListAnswers.FirstOrDefault(i => i.Value == value);
            if (item != null)
                item.Answer = newAnswer;
            else
            {
                newListAnswers.Add(new TextListAnswerViewModel(decimal.Parse(tagName), newAnswer));
            }

            return newListAnswers;
        }*/

        protected TextListAnswerViewModel FindAnswerInModelByTag(string tag)
        {
            decimal answerGuid = decimal.Parse(tag);
            return TypedModel.ListAnswers.FirstOrDefault(a => a.Value == answerGuid);
        }

        protected override void Initialize()
        {
            base.Initialize();
            Orientation = Orientation.Vertical;
            AddItemView = CreateAddListButton();
            AnswersContainer = CreateContainer();
            ActionsContainer = CreateActionContainer(AddItemView);

            llWrapper.AddView(AnswersContainer);
            llWrapper.AddView(ActionsContainer);

            PutAnswerStoredInModelToUI();

            ItemsCount = ListAnswers.Count();
            if (IsMaxAnswerCountExceeded(ItemsCount))
                AddItemView.Enabled = false;
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return FormatSelectedAnswersAsString(ListAnswers);
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            AnswersContainer.RemoveAllViews();

            foreach (TextListAnswerViewModel answer in ListAnswers)
            {
                LinearLayout answerBlock = CreateAnswerBlock(GetAnswerId(answer), GetAnswerTitle(answer));
                AnswersContainer.AddView(answerBlock);
            }
        }

        private Button CreateAddListButton()
        {
            var button = new Button(Context);
            var cbLayoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);

            cbLayoutParams.AddRule(LayoutRules.AlignParentRight);
            button.LayoutParameters = cbLayoutParams;
            button.Text = AddListItemText;
            button.SetTypeface(null, TypefaceStyle.Bold);
            button.Click += AddListItemButtonClick;

            return button;
        }

        private void AddListItemButtonClick(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            //change value generation
            List<TextListAnswerViewModel> listAnswers = GetAnswersFromUI();
            decimal maxValue = listAnswers.Count == 0 ? 0 : listAnswers.Max(i => i.Value);

            LinearLayout newBlock = CreateAnswerBlock((maxValue + 1).ToString(), "");
            AnswersContainer.AddView(newBlock);

            var newEditor = GetFirstChildTypeOf<EditText>(newBlock);
            if (newEditor != null)
            {
                newEditor.RequestFocus();
                ShowKeyboard(newEditor);
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

            var answersToSave = GetAnswersFromUI().Where(item => !String.IsNullOrWhiteSpace(item.Answer) && item.Value != listItemValue).ToArray();
            SaveAnswer(FormatSelectedAnswersAsString(answersToSave), CreateSaveAnswerCommand(answersToSave.ToArray()));
            
            //TextListAnswerViewModel itemFromModel = FindAnswerInModelByTag(buttonTag);

            /*if (itemFromModel != null)
            {
                //TextListAnswerViewModel[] answersToSave = GetNewListAnswersToSave(buttonTag, "", false).ToArray();

                var answersToSave = GetAnswersFromUI().Where(item => !String.IsNullOrWhiteSpace(item.Answer)).ToArray();

                SaveAnswer(FormatSelectedAnswersAsString(answersToSave), CreateSaveAnswerCommand(answersToSave.ToArray()));
            }*/

            RemoveFirstChildByTag(AnswersContainer, buttonTag);
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

                string tag = itemContainer.GetTag(Resource.Id.AnswerId).ToString();
                if (itemTagToRevove == tag)
                {
                    container.RemoveView(itemContainer);
                    return;
                }
            }
        }

        protected LinearLayout CreateContainer()
        {
            var optionsWrapper = new LinearLayout(Context);
            optionsWrapper.Orientation = Orientation.Vertical;
            optionsWrapper.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.FillParent,
                ViewGroup.LayoutParams.FillParent);

            return optionsWrapper;
        }

        protected LinearLayout CreateActionContainer(Button addButton)
        {
            LinearLayout optionsWrapper = CreateContainer();
            var container = new RelativeLayout(Context);
            container.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.FillParent,
                ViewGroup.LayoutParams.FillParent);

            container.AddView(addButton);
            optionsWrapper.AddView(container);
            return optionsWrapper;
        }

        private LinearLayout CreateAnswerBlock(string answerValueTag, string answerTitle)
        {
            var container = new LinearLayout(Context);
            container.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.FillParent,
                ViewGroup.LayoutParams.FillParent);

            container.SetTag(Resource.Id.AnswerId, answerValueTag);

            EditText text = CreateValueEditor(answerValueTag, answerTitle);

            Button button = CreateRemoveListItemButton(answerValueTag);

            container.AddView(text);
            container.AddView(button);

            return container;
        }

        private EditText CreateValueEditor(string answerValueTag, string answerTitle)
        {
            var text = new EditText(Context);
            text.SetSelectAllOnFocus(true);
            text.ImeOptions = ImeAction.Done;
            text.SetSingleLine(true);
            text.FocusChange += textAnswer_FocusChange;
            text.EditorAction += textAnswer_EditorAction;

            var layoutParams = new LayoutParams(ViewGroup.LayoutParams.FillParent,
                ViewGroup.LayoutParams.WrapContent);

            layoutParams.Weight = 1.0f;
            text.LayoutParameters = layoutParams;

            text.Text = answerTitle;
            text.SetTag(Resource.Id.AnswerId, answerValueTag);
            return text;
        }

        private void textAnswer_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                return;
            }

            var editor = sender as EditText;
            if (editor == null)
                return;

            string newAnswer = editor.Text.Trim();
            if (String.IsNullOrWhiteSpace(newAnswer))
                return;

            string tagName = editor.GetTag(Resource.Id.AnswerId).ToString();



            if (!DoesModelHaveItem(tagName, newAnswer))
            {
                if (!IsCommentsEditorFocused)
                    HideKeyboard(editor);

                var answers = GetAnswersFromUI().Where(item => !String.IsNullOrWhiteSpace(item.Answer)).ToArray();
                
                SaveAnswer(FormatSelectedAnswersAsString(answers), CreateSaveAnswerCommand(answers));
            }
        }

        private void textAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            var editor = sender as EditText;
            if (editor != null)
                editor.ClearFocus();
        }

        private Button CreateRemoveListItemButton(string answerTag)
        {
            var button = new Button(Context);

            var layoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);

            button.LayoutParameters = layoutParams;

            button.SetTypeface(null, TypefaceStyle.Bold);
            button.Text = RemoveListItemText;

            button.Click += RemoveListItemClick;
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
            return MaxAnswerCount.HasValue && valueToCheck >= MaxAnswerCount;
        }

        private List<TextListAnswerViewModel> GetAnswersFromUI()
        {
            var answers = new List<TextListAnswerViewModel>();
            for (int i = 0; i < AnswersContainer.ChildCount; i++)
            {
                var itemContainer = AnswersContainer.GetChildAt(i) as ViewGroup;
                var editText = GetFirstChildTypeOf<EditText>(itemContainer);

                if (editText == null)
                    continue;

                decimal listItemValue = decimal.Parse(editText.GetTag(Resource.Id.AnswerId).ToString());

                var item = new TextListAnswerViewModel(listItemValue.ToString(), editText.Text.Trim());

                answers.Add(item);
            }
            return answers;
        }

        private string FormatSelectedAnswersAsString(IEnumerable<TextListAnswerViewModel> selectedAnswers)
        {
            return string.Join(",", selectedAnswers.Select(GetAnswerTitle));
        }
    }
}