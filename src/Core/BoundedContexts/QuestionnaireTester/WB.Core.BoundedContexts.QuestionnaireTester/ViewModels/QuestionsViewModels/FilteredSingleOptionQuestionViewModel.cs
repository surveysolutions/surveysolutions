using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class FilteredSingleOptionQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        public class FilteredComboboxItemViewModel 
        {
            public string Text { get; set; }
            public decimal Value { get; set; }

            public override string ToString()
            {
                return Text.Replace("</b>", "").Replace("<b>", "");
            }
        }

        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private Identity questionIdentity;
        private Guid interviewId;
        public QuestionStateViewModel<SingleOptionQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }
        public IList<FilteredComboboxItemViewModel> Options { get; set; }


        public FilteredSingleOptionQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }



        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = (FilteredSingleOptionQuestionModel)questionnaire.Questions[entityIdentity.Id];
            var answerModel = interview.GetSingleOptionAnswer(entityIdentity);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.Options = questionModel
                .Options
                .Select(this.ToViewModel)
                .ToList();

            if (answerModel != null)
            {
                var selectedValue = answerModel.Answer;
                SelectedObject = Options.SingleOrDefault(i => i.Value == selectedValue);
            }
        }

        private FilteredComboboxItemViewModel ToViewModel(OptionModel model)
        {
            var optionViewModel = new FilteredComboboxItemViewModel
            {
                Text = model.Title,
                Value = model.Value
            };

            return optionViewModel;
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand<string>(SendAnswerFilteredComboboxQuestionCommand)); }
        }

        private FilteredComboboxItemViewModel selectedObject;
        public FilteredComboboxItemViewModel SelectedObject
        {
            get { return this.selectedObject; }
            set
            {
                this.selectedObject = value;
                RaisePropertyChanged();
            }
        }

        private string filterText;
        public string FilterText
        {
            get { return this.filterText; }
            set
            {
                if (value.IsNullOrEmpty())
                {
                    this.filterText = null;
                    SetSuggestionsEmpty();
                    return;
                }
                    
                this.filterText = value;

                var list = this.GetSuggestionsList(this.filterText).ToList();

                if (list.Any())
                {
                    AutoCompleteSuggestions = list;
                }
                else
                {
                    SetSuggestionsEmpty();
                }
            }
        }

        private IEnumerable<FilteredComboboxItemViewModel> GetSuggestionsList(string textHint)
        {
            var upperTextHint = textHint.ToUpper();

            foreach (var model in Options)
            {
                if (model.Text.IsNullOrEmpty())
                    continue;

                string upperText = model.Text.ToUpper();

                var index = upperText.IndexOf(upperTextHint, StringComparison.CurrentCulture);
                if (index >= 0)
                {
                    yield return model;
                    yield return new FilteredComboboxItemViewModel()
                    {
                        Text = model.Text.Insert(index + textHint.Length, "</b>").Insert(index, "<b>"),
                        Value = model.Value
                    };
                }
            }
        }

        private void SetSuggestionsEmpty()
        {
            AutoCompleteSuggestions = new List<FilteredComboboxItemViewModel>();
        }

        private List<FilteredComboboxItemViewModel> autoCompleteSuggestions = new List<FilteredComboboxItemViewModel>();
        public List<FilteredComboboxItemViewModel> AutoCompleteSuggestions
        {
            get
            {
                if (this.autoCompleteSuggestions == null)
                {
                    this.autoCompleteSuggestions = new List<FilteredComboboxItemViewModel>();
                }
                return this.autoCompleteSuggestions;
            }
            set { this.autoCompleteSuggestions = value; RaisePropertyChanged(); }
        }

        private async void SendAnswerFilteredComboboxQuestionCommand(string text)
        {
            var answerViewModel = this.Options.SingleOrDefault(i => i.Text == text);

            if (answerViewModel == null)
            {
                this.QuestionState.Validity.MarkAnswerAsInvalidWithMessage(UIResources.Interview_Question_Text_MaskError);
                return;
            }

            var answerValue = answerViewModel.Value;

            var command = new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.principal.CurrentUserIdentity.UserId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                answerValue);

            try
            {
                await this.Answering.SendAnswerQuestionCommand(command);

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }
    }
}