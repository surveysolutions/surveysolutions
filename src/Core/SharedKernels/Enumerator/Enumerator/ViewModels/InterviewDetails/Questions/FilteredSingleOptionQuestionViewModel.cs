using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class FilteredSingleOptionQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ICompositeQuestion,
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

            public override bool Equals(object obj)
            {
                var secondObj = obj as FilteredComboboxItemViewModel;
                if (secondObj == null) return false;
                return Equals(secondObj);
            }

            protected bool Equals(FilteredComboboxItemViewModel other)
            {
                return string.Equals(this.Text, other.Text) && this.Value == other.Value;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((this.Text?.GetHashCode() ?? 0) * 397) ^ this.Value.GetHashCode();
                }
            }
        }

        private const int SuggestionsMaxCount = 15;

        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;

        private Identity questionIdentity;
        private Guid interviewId;
        protected IStatefulInterview interview;

        public IQuestionStateViewModel QuestionState => this.questionState;

        public AnsweringViewModel Answering { get; private set; }
        public QuestionInstructionViewModel InstructionViewModel { get; set; }

        public FilteredSingleOptionQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            FilteredOptionsViewModel filteredOptionsViewModel)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (interviewRepository == null) throw new ArgumentNullException(nameof(interviewRepository));

            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.questionState = questionStateViewModel;
            this.Answering = answering;
            this.InstructionViewModel = instructionViewModel;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity, SuggestionsMaxCount);
            this.filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;

            interview = this.interviewRepository.Get(interviewId);
            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.UpdateOptionsState();

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs) => this.UpdateOptionsState();

        private void UpdateOptionsState()
        {
            var singleOptionQuestion = this.interview.GetSingleOptionQuestion(this.questionIdentity);

            if (singleOptionQuestion.IsAnswered)
            {
                var selectedValue = singleOptionQuestion.GetAnswer().SelectedValue;
                var answerOption = ToViewModel(this.interview.GetOptionForQuestionWithoutFilter(this.questionIdentity, selectedValue));
                this.SelectedObject = answerOption;
                this.DefaultText = answerOption == null ? String.Empty : answerOption.Text;
                this.ResetTextInEditor = this.DefaultText;
            }
            else
            {
                this.UpdateAutoCompleteList();
            }
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
        
        public IMvxCommand ValueChangeCommand => new MvxCommand<string>(this.SendAnswerFilteredComboboxQuestionCommand);
        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);

        private async Task RemoveAnswerAsync()
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.principal.CurrentUserIdentity.UserId,
                        this.questionIdentity,
                        DateTime.UtcNow));

                this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.ClearText();
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            if (@event.Questions.Contains(this.questionIdentity))
            {
                this.InvokeOnMainThread(this.ClearText);
            }
        }

        private void ClearText()
        {
            this.ResetTextInEditor = string.Empty;
            this.DefaultText = null;
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
                if (this.filterText == value)
                    return;

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
            var options = this.filteredOptionsViewModel.GetOptions(searchFor)
                .Select(this.ToViewModel)
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
        private readonly QuestionStateViewModel<SingleOptionQuestionAnswered> questionState;

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
            var answerCategoricalOption = this.interview.GetOptionForQuestionWithFilter(this.questionIdentity, text);

            if (answerCategoricalOption == null)
            {
                var errorMessage = UIResources.Interview_Question_Filter_MatchError.FormatString(text);
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(errorMessage);
                return;
            }

            var answerValue = answerCategoricalOption.Value;

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

                this.FilterText = answerCategoricalOption.Title;
                this.DefaultText = answerCategoricalOption.Title;
                this.resetTextInEditor = answerCategoricalOption.Title;
                this.selectedObject = ToViewModel(answerCategoricalOption);

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Dispose()
        {
            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;

            this.filteredOptionsViewModel.Dispose();
            this.QuestionState.Dispose();
            this.eventRegistry.Unsubscribe(this);
        }
    }
}