using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class FilteredSingleOptionQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel, 
        ILiteEventHandler<AnswerRemoved>,
        IDisposable
    {
        public class FilteredComboboxItemViewModel 
        {
            public string Text { get; set; }
            public decimal Value { get; set; }

            public override string ToString()
            {
                return this.Text.Replace("</b>", "").Replace("<b>", "");
            }
        }

        private const int SuggestionsMaxCount = 15;

        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IOptionsRepository optionsRepository;

        private Identity questionIdentity;
        private QuestionnaireIdentity questionnaireIdentity;
        private Guid interviewId;
        protected IStatefulInterview interview;
        public QuestionStateViewModel<SingleOptionQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }
        private List<FilteredComboboxItemViewModel> Options { get; set; }

        public FilteredSingleOptionQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering, 
            IOptionsRepository optionsRepository)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
            this.optionsRepository = optionsRepository;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetSingleOptionAnswer(entityIdentity);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;
            this.questionnaireIdentity = interview.QuestionnaireIdentity;
            
            //should be removed
            this.Options = optionsRepository.GetQuestionOptions(interview.QuestionnaireIdentity, this.questionIdentity.Id)
                .Select(this.ToViewModel)
                .ToList();

            if (answerModel.IsAnswered)
            {
                var selectedValue = answerModel.Answer;
                FilteredComboboxItemViewModel answerOption = 
                    this.ToViewModel(optionsRepository.GetQuestionOption(interview.QuestionnaireIdentity, this.questionIdentity.Id, Convert.ToInt32(selectedValue)));  
                this.SelectedObject = answerOption;
                this.DefaultText = answerOption == null ? String.Empty : answerOption.Text;
                this.ResetTextInEditor = this.DefaultText;
            }
            else
            {
                UpdateAutoCompleteList();
            }
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private FilteredComboboxItemViewModel ToViewModel(CategoricalOption model)
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
            get { return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand<string>(this.SendAnswerFilteredComboboxQuestionCommand)); }
        }

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return new MvxCommand(async () =>
                {
                    try
                    {
                        await this.Answering.SendRemoveAnswerCommandAsync(
                            new RemoveAnswerCommand(this.interviewId,
                                this.principal.CurrentUserIdentity.UserId,
                                this.questionIdentity,
                                DateTime.UtcNow));

                        this.QuestionState.Validity.ExecutedWithoutExceptions();
                    }
                    catch (InterviewException exception)
                    {
                        this.QuestionState.Validity.ProcessException(exception);
                    }
                });
            }
        }

        public void Handle(AnswerRemoved @event)
        {
            if (this.questionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                InvokeOnMainThread(() =>
                {
                    this.ResetTextInEditor = string.Empty;
                    this.DefaultText = null;
                });
            }
        }

        private FilteredComboboxItemViewModel selectedObject;
        public FilteredComboboxItemViewModel SelectedObject
        {
            get { return this.selectedObject; }
            set
            {
                this.selectedObject = value;
                this.RaisePropertyChanged();
            }
        }

        public string DefaultText { get; set; }

        private string filterText;
        public string FilterText
        {
            get { return this.filterText; }
            set
            {
                this.filterText = value;

                this.UpdateAutoCompleteList();

                this.RaisePropertyChanged();
            }
        }

        private void UpdateAutoCompleteList()
        {
            var list = this.GetSuggestionsList(this.filterText).ToList();

            if (list.Any())
            {
                this.AutoCompleteSuggestions = list;
            }
            else
            {
                this.SetSuggestionsEmpty();
            }
        }

        private string resetTextInEditor;
        public string ResetTextInEditor
        {
            get { return this.resetTextInEditor; }
            set
            {
                this.resetTextInEditor = value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.SelectedObject = null;
                    this.FilterText = null;
                }
                this.RaisePropertyChanged();
            }
        }

        private IEnumerable<FilteredComboboxItemViewModel> GetSuggestionsList(string searchFor)
        {
            var options = interview.GetFilteredOptionsForQuestion(this.questionIdentity, null, searchFor)
                .Select(this.ToViewModel)
                .Take(SuggestionsMaxCount)
                .ToList();

            foreach (var model in options)
            {
                if (model.Text.IsNullOrEmpty())
                    continue;

                if (searchFor != null)
                {
                    //Insert and IndexOf with culture specific search cannot be used together 
                    //http://stackoverflow.com/questions/4923187/string-indexof-and-replace
                    var startIndexOfSearchedText = model.Text.IndexOf(searchFor, StringComparison.OrdinalIgnoreCase);
                    if (startIndexOfSearchedText >= 0)
                    {
                        yield return new FilteredComboboxItemViewModel
                        {
                            Text = model.Text.Insert(startIndexOfSearchedText + searchFor.Length, "</b>")
                                             .Insert(startIndexOfSearchedText, "<b>"),
                            Value = model.Value
                        };
                    }
                }
                else
                {
                    yield return new FilteredComboboxItemViewModel
                    {
                        Text = model.Text,
                        Value = model.Value
                    };
                }
            }
        }

        private void SetSuggestionsEmpty()
        {
            this.AutoCompleteSuggestions = new List<FilteredComboboxItemViewModel>();
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
            set { this.autoCompleteSuggestions = value; this.RaisePropertyChanged(); }
        }

        private async void SendAnswerFilteredComboboxQuestionCommand(string text)
        {
            FilteredComboboxItemViewModel answerViewModel = this.Options.SingleOrDefault(i => i.Text == text);

            if (answerViewModel == null)
            {
                var errorMessage = UIResources.Interview_Question_Filter_MatchError.FormatString(text);
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(errorMessage);
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
                await this.Answering.SendAnswerQuestionCommandAsync(command);

                this.FilterText = answerViewModel.Text;
                this.DefaultText = answerViewModel.Text;
                this.resetTextInEditor = answerViewModel.Text;
                this.selectedObject = answerViewModel;

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Dispose()
        {
            this.QuestionState.Dispose();
            this.eventRegistry.Unsubscribe(this);
        }
    }
}