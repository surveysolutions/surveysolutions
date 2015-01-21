using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using global::Android.Views;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class FilteredComboboxQuestionView : AbstractQuestionView
    {
        protected AutoCompleteTextView filteredCombobox;
        public FilteredComboboxQuestionView(
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

            InitializeFilteredCombobox();
            PutAnswerStoredInModelToUI();
        }

        private void InitializeFilteredCombobox()
        {
            this.filteredCombobox = new AutoCompleteTextView(this.CurrentContext) { Threshold = 1, ImeOptions = ImeAction.Done };

            this.filteredCombobox.SetSelectAllOnFocus(true);
            this.filteredCombobox.SetSingleLine(true);
            this.filteredCombobox.ItemClick += filteredCombobox_ItemClick;
            this.filteredCombobox.FocusChange += this.filteredCombobox_FocusChange;
            this.filteredCombobox.KeyPress += FilteredComboboxOnKeyPress;
            this.filteredCombobox.EditorAction += FilteredComboboxOnEditorAction;
            

            var adapter = new ArrayAdapter<String>(this.CurrentContext, Resource.Layout.FilteredComboboxRowLayout,
                this.Answers.Select(option => option.Title).ToList());
            this.filteredCombobox.Adapter = adapter;
            
            this.llWrapper.AddView(this.filteredCombobox);

            this.llWrapper.Focusable = true;
            this.llWrapper.FocusableInTouchMode = true;
        }

        private void FilteredComboboxOnEditorAction(object sender, TextView.EditorActionEventArgs editorActionEventArgs)
        {
            if (editorActionEventArgs.ActionId == ImeAction.Done)
            {
                this.filteredCombobox.ClearFocus();
            }
            else
            {
                editorActionEventArgs.Handled = false;
            }
        }

        private void FilteredComboboxOnKeyPress(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = false;
            if (keyEventArgs.KeyCode == Keycode.Del && string.IsNullOrEmpty(this.filteredCombobox.Text))
            {
                this.filteredCombobox.ShowDropDown();
            }
        }

        private void filteredCombobox_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                this.filteredCombobox.ShowDropDown();
            }
            else
            {
                SaveAnswerOrShowErrorOnUi();
            }
        }

        void filteredCombobox_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            this.filteredCombobox.ClearFocus();
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
                this.ShowErrorMessageOnUi(string.Format(errorTemplate, this.filteredCombobox.Text));
            }
        }

        private AnswerViewModel FindSelectedAnswer()
        {
            return this.Answers.FirstOrDefault(option => option.Title.ToLower().Equals(this.filteredCombobox.Text.ToLower()));
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
            this.filteredCombobox.Text = GetAnswerStoredInModelAsString();
        }

        private FilteredComboboxQuestionViewModel TypedMode
        {
            get { return this.Model as FilteredComboboxQuestionViewModel; }
        }

        private IEnumerable<AnswerViewModel> Answers
        {
            get { return this.TypedMode.Answers; }
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
    }
}