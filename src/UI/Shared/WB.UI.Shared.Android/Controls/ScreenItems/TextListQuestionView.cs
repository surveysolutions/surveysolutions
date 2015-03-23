using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class TextListQuestionView : AbstractQuestionView
    {
        private LinearLayout AnswersContainer;
        private Button AddTextListItemButton;

        private int ItemsCountInUI;

        private List<decimal> answersTreatedAsSaved;
        private string valueBeforeEditing;

        private const string AddListItemText = "+";
        private const string RemoveListItemText = "-";

        public TextListQuestionView(Context context, 
            IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source, 
            Guid questionnairePublicKey,
            ICommandService commandService, 
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService,
                membership)
        {
        }

        private IEnumerable<TextListAnswerViewModel> ListAnswers
        {
            get { return TypedModel.ListAnswers; }
        }

        private TextListQuestionViewModel TypedModel
        {
            get { return Model as TextListQuestionViewModel; }
        }

        private int? MaxAnswerCount
        {
            get { return TypedModel.MaxAnswerCount; }
        }

        private int MaxAnswerCountLimit
        {
            get { return TypedModel.MaxAnswerCountLimit; }
        }

        private void TextAnswer_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            var editor = sender as EditText;
            if (editor == null) 
                return;
            if (e.ActionId != ImeAction.Done) 
                return;

            this.HideKeyboard(editor);
            editor.ClearFocus();
        }
        
        private async void RemoveTextListItemButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            string buttonTag = button.GetTag(Resource.Id.AnswerId).ToString();
            decimal listItemValue = decimal.Parse(buttonTag); 
            
            if (answersTreatedAsSaved.Contains(listItemValue))
            {
                bool result = await ConfirmRosterDecreaseAsync(Model.TriggeredRosters, 1);
                if (!result)
                    return;

                TextListAnswerViewModel[] answersToSave =
                    GetAnswersFromUI()
                        .Where(item => !String.IsNullOrWhiteSpace(item.Answer) && item.Value != listItemValue)
                        .ToArray();

                SaveAnswer(FormatSelectedAnswersAsString(answersToSave),
                    CreateSaveAnswerCommand(answersToSave.ToArray()));

                answersTreatedAsSaved.Remove(listItemValue);
            }

            RemoveFirstChildByTag(AnswersContainer, buttonTag);
            ItemsCountInUI--;

            AddTextListItemButton.Visibility = IsAddingAnswerAllowed(ItemsCountInUI) ? 
                                               ViewStates.Visible :
                                               ViewStates.Gone;
        }

        private void TextListItemEditor_FocusChange(object sender, FocusChangeEventArgs e)
        {
            var editor = sender as EditText;
            if (editor == null)
                return;

            string newAnswer = editor.Text.Trim();
            if (e.HasFocus)
            {
                this.valueBeforeEditing = newAnswer;
                return;
            }
            
            string tagName = editor.GetTag(Resource.Id.AnswerId).ToString();
            decimal answerValue = decimal.Parse(tagName);
            
            if (!answersTreatedAsSaved.Contains(answerValue)) // new value, only create
            {
                if (!string.IsNullOrWhiteSpace(newAnswer))
                {
                    SaveState(editor);
                    answersTreatedAsSaved.Add(answerValue);
                }
            }
            else // old value, may be deleted or updated
            {
                if (!string.IsNullOrWhiteSpace(newAnswer)) // value should be deleted 
                {

                    if (this.valueBeforeEditing != newAnswer)
                    {
                        this.SaveState(editor);
                    }
                }
                else
                {
                    this.SaveState(editor);
                    answersTreatedAsSaved.Remove(answerValue); 
                }
            }
        }

        private void CreateNewTextListItemButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            if(!IsAddingAnswerAllowed(ItemsCountInUI))
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
                this.ShowKeyboard(newEditor);
            }

            ItemsCountInUI++;

            AddTextListItemButton.Visibility = IsAddingAnswerAllowed(ItemsCountInUI) ?
                                   ViewStates.Visible :
                                   ViewStates.Gone;
        }

        protected override void Initialize()
        {
            base.Initialize();
            answersTreatedAsSaved = new List<decimal>();

            Orientation = Orientation.Vertical;
            AddTextListItemButton = CreateAddListItemButton();
            AnswersContainer = CreateContainer();
            
            var actionsContainer = CreateActionContainer(AddTextListItemButton);

            llWrapper.AddView(AnswersContainer);
            llWrapper.AddView(actionsContainer);

            PutAnswerStoredInModelToUI();

            ItemsCountInUI = ListAnswers.Count();

            AddTextListItemButton.Visibility = IsAddingAnswerAllowed(ItemsCountInUI) ?
                                   ViewStates.Visible :
                                   ViewStates.Gone;
        }

        private Button CreateRemoveListItemButton(string answerTag)
        {
            var removeTextListItemButton = new Button(CurrentContext);

            var layoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);

            removeTextListItemButton.LayoutParameters = layoutParams;
            removeTextListItemButton.SetTypeface(null, TypefaceStyle.Bold);
            removeTextListItemButton.Text = RemoveListItemText;
            removeTextListItemButton.Click += RemoveTextListItemButton_Click;
            removeTextListItemButton.SetTag(Resource.Id.AnswerId, answerTag);

            return removeTextListItemButton;
        }

        private Button CreateAddListItemButton()
        {
            var createNewTextListItemButton = new Button(CurrentContext);

            var cbLayoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);

            cbLayoutParams.AddRule(LayoutRules.AlignParentRight);
            createNewTextListItemButton.LayoutParameters = cbLayoutParams;
            createNewTextListItemButton.Text = AddListItemText;
            createNewTextListItemButton.SetTypeface(null, TypefaceStyle.Bold);
            createNewTextListItemButton.Click += CreateNewTextListItemButton_Click;

            return createNewTextListItemButton;
        }

        private LinearLayout CreateContainer()
        {
            var optionsWrapper = new LinearLayout(CurrentContext);
            optionsWrapper.Orientation = Orientation.Vertical;
            optionsWrapper.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.FillParent,
                ViewGroup.LayoutParams.FillParent);

            return optionsWrapper;
        }

        private LinearLayout CreateActionContainer(Button addButton)
        {
            LinearLayout optionsWrapper = CreateContainer();
            var container = new RelativeLayout(CurrentContext);
            container.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.FillParent,
                ViewGroup.LayoutParams.FillParent);

            container.AddView(addButton);
            optionsWrapper.AddView(container);
            return optionsWrapper;
        }

        private LinearLayout CreateAnswerBlock(string answerValueTag, string answerTitle)
        {
            var container = new LinearLayout(CurrentContext);
            container.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.FillParent,
                ViewGroup.LayoutParams.FillParent);

            container.SetTag(Resource.Id.AnswerId, answerValueTag);

            EditText textListEditor = CreateValueEditor(answerValueTag, answerTitle);

            Button removeTextListItemButton = CreateRemoveListItemButton(answerValueTag);

            container.AddView(textListEditor);
            container.AddView(removeTextListItemButton);

            return container;
        }

        private EditText CreateValueEditor(string answerValueTag, string answerTitle)
        {
            var textListItemEditor = new EditText(CurrentContext);
            textListItemEditor.SetSelectAllOnFocus(true);
            textListItemEditor.ImeOptions = ImeAction.Done;
            textListItemEditor.SetSingleLine(true);
            textListItemEditor.FocusChange += TextListItemEditor_FocusChange;
            textListItemEditor.EditorAction += TextAnswer_EditorAction;

            var layoutParams = new LayoutParams(ViewGroup.LayoutParams.FillParent,
                ViewGroup.LayoutParams.WrapContent);

            layoutParams.Weight = 1.0f;
            textListItemEditor.LayoutParameters = layoutParams;

            textListItemEditor.Text = answerTitle;
            textListItemEditor.SetTag(Resource.Id.AnswerId, answerValueTag);
            return textListItemEditor;
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
                LinearLayout answerBlock = CreateAnswerBlock(answer.Value.ToString(), answer.Answer);
                AnswersContainer.AddView(answerBlock);

                answersTreatedAsSaved.Add(answer.Value);
            }
        }

        private void RemoveFirstChildByTag(LinearLayout container, string itemTagToRemove)
        {
            if (container == null)
                return;

            ViewGroup childToRemove = container.GetChildren()
                .Where(child => child is ViewGroup)
                .Cast<ViewGroup>()
                .SingleOrDefault(view => view.GetTag(Resource.Id.AnswerId).ToString() == itemTagToRemove);

            if (childToRemove != null)
                container.RemoveView(childToRemove);
        }

        private void SaveState(EditText editor)
        {
            TextListAnswerViewModel[] answers = GetAnswersFromUI().Where(item => !string.IsNullOrWhiteSpace(item.Answer)).ToArray();

            SaveAnswer(FormatSelectedAnswersAsString(answers), CreateSaveAnswerCommand(answers));
        }
        
        private T GetFirstChildTypeOf<T>(ViewGroup layout) where T : class
        {
            if (layout == null)
                return null;

            for (int childIndex = 0; childIndex < layout.ChildCount; childIndex++)
            {
                var child = layout.GetChildAt(childIndex) as T;
                if (child != null)
                    return child;
            }

            return null;
        }

        private bool IsAddingAnswerAllowed(int valueToCheck)
        {
            if (valueToCheck >= MaxAnswerCountLimit)
                return false;

            if (MaxAnswerCount.HasValue)
                return valueToCheck < MaxAnswerCount;

            return true;
        }

        private List<TextListAnswerViewModel> GetAnswersFromUI()
        {
            var answers = new List<TextListAnswerViewModel>();
            for (int childindex = 0; childindex < AnswersContainer.ChildCount; childindex++)
            {
                var itemContainer = AnswersContainer.GetChildAt(childindex) as ViewGroup;
                var editText = GetFirstChildTypeOf<EditText>(itemContainer);

                if (editText == null)
                    continue;

                decimal listItemValue = decimal.Parse(editText.GetTag(Resource.Id.AnswerId).ToString());

                var item = new TextListAnswerViewModel(listItemValue, editText.Text.Trim());

                answers.Add(item);
            }

            return answers;
        }

        private AnswerQuestionCommand CreateSaveAnswerCommand(TextListAnswerViewModel[] selectedAnswers)
        {
            List<Tuple<decimal, string>> answers =
                selectedAnswers.Select(a => new Tuple<decimal, string>(a.Value, a.Answer)).ToList();

            return new AnswerTextListQuestionCommand(QuestionnairePublicKey, Membership.CurrentUser.Id,
                Model.PublicKey.Id, Model.PublicKey.InterviewItemPropagationVector, DateTime.UtcNow, answers.ToArray());
        }

        private string FormatSelectedAnswersAsString(IEnumerable<TextListAnswerViewModel> selectedAnswers)
        {
            return string.Join(",", selectedAnswers.Select(answer => answer.Answer));
        }
    }
}