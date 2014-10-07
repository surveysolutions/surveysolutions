using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
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
            this.filteredCombobox = new AutoCompleteTextView(this.Context);
            
            this.filteredCombobox.Threshold = 1;
            this.filteredCombobox.ImeOptions = ImeAction.Done;
            this.filteredCombobox.SetSelectAllOnFocus(true);
            this.filteredCombobox.SetSingleLine(true);
            this.filteredCombobox.ItemClick += filteredCombobox_ItemClick;

            var adapter = new ArrayAdapter<String>(this.Context, Resource.Layout.FilteredComboboxRowLayout,
                this.Answers.Select(option => option.Title).ToList());
            this.filteredCombobox.Adapter = adapter;
            
            this.llWrapper.AddView(this.filteredCombobox);
        }

        void filteredCombobox_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var answer = this.Answers.FirstOrDefault(option=> option.Title.Equals(this.filteredCombobox.Text));
            if (answer != null)
            {
                this.SaveAnswer(this.GetAnswerTitle(answer), this.CreateSaveAnswerCommand(answer));
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