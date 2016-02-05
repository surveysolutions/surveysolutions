using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
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

        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;

        private Identity questionIdentity;
        private Guid interviewId;
        public QuestionStateViewModel<SingleOptionQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }
        public List<FilteredComboboxItemViewModel> Options { get; set; }

        public FilteredSingleOptionQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        public Identity Identity { get { return this.questionIdentity; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = questionnaire.GetQuestion<FilteredSingleOptionQuestionModel>(entityIdentity.Id);
            var answerModel = interview.GetSingleOptionAnswer(entityIdentity);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.Options = questionModel
                .Options
                .Select(this.ToViewModel)
                .ToList();

            if (answerModel.IsAnswered)
            {
                var selectedValue = answerModel.Answer;
                FilteredComboboxItemViewModel answerOption = this.Options.SingleOrDefault(i => i.Value == selectedValue);
                this.SelectedObject = answerOption;
                this.DefaultText = answerOption == null ? String.Empty : answerOption.Text;
                this.ResetTextInEditor = this.DefaultText;
            }
            else
            {
                this.AutoCompleteSuggestions = this.Options;
            }
            this.eventRegistry.Subscribe(this, interviewId);
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
                    this.QuestionState.IsAnswered = false;
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

                var list = this.GetSuggestionsList(this.filterText).ToList();

                if (list.Any())
                {
                    this.AutoCompleteSuggestions = list;
                }
                else
                {
                    this.SetSuggestionsEmpty();
                }

                this.RaisePropertyChanged();
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
            

            foreach (var model in this.Options)
            {
                if (model.Text.IsNullOrEmpty())
                    continue;

                if (searchFor != null)
                {
                    var startIndexOfSearchedText = model.Text.IndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase);
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
            this.eventRegistry.Unsubscribe(this, interviewId.FormatGuid());
        }
    }
}