using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class CascadingComboboxQuestionView : AbstractQuestionView
    {
        public CascadingComboboxQuestionView(
            Context context,
            IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source,
            Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
            this.Model.PropertyChanged += this.Model_PropertyChanged;
        }

        protected AutoCompleteTextView cascadingCombobox;
        private ArrayAdapter<string> adapter;


        protected override void Initialize()
        {
            base.Initialize();

            InitializeFilteredCombobox();
            PutAnswerStoredInModelToUI();
        }

        private void InitializeFilteredCombobox()
        {
            this.cascadingCombobox = new AutoCompleteTextView(this.CurrentContext)
            {
                Threshold = 0, 
                ImeOptions = ImeAction.Done
            };

            this.cascadingCombobox.SetSelectAllOnFocus(true);
            this.cascadingCombobox.SetSingleLine(true);
            this.cascadingCombobox.ItemClick += this.cascadingCombobox_ItemClick;
            this.cascadingCombobox.FocusChange += this.cascadingCombobox_FocusChange;
            this.cascadingCombobox.KeyPress += CascadingComboboxOnKeyPress;
            this.cascadingCombobox.EditorAction += CascadingComboboxOnEditorAction;

            this.adapter = new ArrayAdapter<String>(
                this.CurrentContext, 
                Resource.Layout.CascadingComboboxRowLayout,
                this.Answers.Select(option => option.Title).ToList());

            this.cascadingCombobox.Adapter = this.adapter;
            
            this.llWrapper.AddView(this.cascadingCombobox);
        }

        private void CascadingComboboxOnEditorAction(object sender, TextView.EditorActionEventArgs editorActionEventArgs)
        {
            if (editorActionEventArgs.ActionId == ImeAction.Done)
            {
                this.cascadingCombobox.ClearFocus();
            }
            else
            {
                editorActionEventArgs.Handled = false;
            }
        }

        private void CascadingComboboxOnKeyPress(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = false;
            if (keyEventArgs.KeyCode == Keycode.Del && string.IsNullOrEmpty(this.cascadingCombobox.Text))
            {
                this.cascadingCombobox.ShowDropDown();
            }
        }

        private void cascadingCombobox_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                this.cascadingCombobox.ShowDropDown();
            }
            else
            {
                SaveAnswerOrShowErrorOnUi();
            }
        }

        void cascadingCombobox_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            this.cascadingCombobox.ClearFocus();
        }

        private void SaveAnswerOrShowErrorOnUi()
        {
            var answer = FindSelectedAnswer();
            if (answer != null)
            {
                if (GetAnswerTitle(answer) != this.Model.AnswerString)
                {
                    this.SaveAnswer(this.GetAnswerTitle(answer), this.CreateSaveAnswerCommand(answer));
                }
            }
            else
            {
                var errorTemplate = Resources.GetText(Resource.String.AnswerIsNotPresentInFilteredComboboxOptionsList);
                this.ShowErrorMessageOnUi(string.Format(errorTemplate, this.cascadingCombobox.Text));
            }
        }

        private AnswerViewModel FindSelectedAnswer()
        {
            return this.Answers.FirstOrDefault(option => option.Title.ToLower().Equals(this.cascadingCombobox.Text.ToLower()));
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool answerOptionsChanged = e.PropertyName == "AnswerOptions";
            if (answerOptionsChanged)
            {
                adapter.Clear();
                adapter.AddAll(this.Answers.Select(option => option.Title).ToList());
                this.cascadingCombobox.DismissDropDown();
                this.cascadingCombobox.ClearListSelection();
                this.cascadingCombobox.Text = string.Empty;
            }
            bool answerRemovedChanged = e.PropertyName == "AnswerRemoved";
            if (answerRemovedChanged)
            {
                this.cascadingCombobox.Text = string.Empty;
            }
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            var selectedAnswer = this.Answers.SingleOrDefault(this.IsAnswerSelected);

            return selectedAnswer != null
                ? this.GetAnswerTitle(selectedAnswer)
                : string.Empty;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            this.cascadingCombobox.Text = GetAnswerStoredInModelAsString();
        }

        private CascadingComboboxQuestionViewModel TypedMode
        {
            get { return this.Model as CascadingComboboxQuestionViewModel; }
        }

        private IEnumerable<AnswerViewModel> Answers
        {
            get { return this.TypedMode.AnswerOptions; }
        }

        private bool IsAnswerSelected(AnswerViewModel answer)
        {
            return answer.Selected;
        }

        private string GetAnswerTitle(AnswerViewModel answer)
        {
            return answer.Title;
        }

        private AnswerQuestionCommand CreateSaveAnswerCommand(AnswerViewModel selectedAnswer)
        {
           return new AnswerSingleOptionQuestionCommand(this.QuestionnairePublicKey,
                this.Membership.CurrentUser.Id,
                this.Model.PublicKey.Id, this.Model.PublicKey.InterviewItemPropagationVector, DateTime.UtcNow,
                selectedAnswer.Value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Model != null)
                {
                    this.Model.PropertyChanged -= this.Model_PropertyChanged;
                }
            }

            base.Dispose(disposing);
        }
    }
}