using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public abstract class BaseFilteredQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>, 
        ICompositeQuestion, 
        IDisposable
    {
        protected const int SuggestionsMaxCount = 50;

        protected readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;

        protected BaseFilteredQuestionViewModel(
            IPrincipal principal,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry)
        {
            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
            this.InstructionViewModel = instructionViewModel;
        }

        protected Guid interviewId;
        protected IStatefulInterview interview;
        private int? answer;

        public Identity Identity { get; private set; }

        public IQuestionStateViewModel QuestionState { get; }
        public AnsweringViewModel Answering { get; }
        public QuestionInstructionViewModel InstructionViewModel { get; }

        private string filterText;
        public string FilterText
        {
            get => this.filterText;
            set
            {
                this.filterText = value;
                this.RaisePropertyChanged();
            }
        }

        private List<string> autoCompleteSuggestions = new List<string>();
        public List<string> AutoCompleteSuggestions
        {
            get => this.autoCompleteSuggestions;
            set => this.RaiseAndSetIfChanged(ref this.autoCompleteSuggestions, value);
        }

        public IMvxAsyncCommand<string> FilterCommand => new MvxAsyncCommand<string>(this.UpdateFilterAndSaveIfExactMatchWithAnyOptionAsync);
        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);
        public IMvxAsyncCommand<string> SaveAnswerBySelectedOptionCommand => new MvxAsyncCommand<string>(this.SaveAnswerBySelectedOptionAsync);
        public IMvxCommand ShowErrorIfNoAnswerCommand => new MvxCommand(this.ShowErrorIfNoAnswer);

        protected abstract IEnumerable<CategoricalOption> GetSuggestions(string filter);
        protected abstract CategoricalOption GetAnsweredOption(int answer);
        protected abstract CategoricalOption GetOptionByFilter(string filter);
        protected virtual void Initialize(string interviewId, Identity entityIdentity, NavigationState navigationState) { }

        public virtual void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.Identity = entityIdentity;
            this.interview = this.interviewRepository.Get(interviewId);
            this.interviewId = this.interview.Id;

            ((QuestionStateViewModel<SingleOptionQuestionAnswered>)this.QuestionState).Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);

            this.Initialize(interviewId, entityIdentity, navigationState);
            Task.Run(this.SetAnswerAndUpdateFilter).WaitAndUnwrapException();

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private IEnumerable<string> GetHighlightedSuggestions(string filter)
        {
            foreach (var model in this.GetSuggestions(filter))
            {
                if (model.Title.IsNullOrEmpty())
                    continue;

                yield return this.GetHighlightedText(model.Title, filter);
            }
        }

        private void ShowErrorIfNoAnswer()
        {
            if (string.IsNullOrEmpty(this.FilterText)) return;

            var selectedOption = this.GetOptionByFilter(this.FilterText);

            if (selectedOption != null) return;

            var errorMessage = UIResources.Interview_Question_Filter_MatchError.FormatString(this.FilterText);
            this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(errorMessage);
        }

        private async Task SaveAnswerBySelectedOptionAsync(string optionText)
        {
            optionText = this.RemoveHighlighting(optionText);

            await this.UpdateFilterAndSuggestionsAsync(optionText);
            await this.SaveAnswerAsync(optionText);
        }

        protected virtual async Task SaveAnswerAsync(string optionText)
        {
            //if app crashed and automatically restored 
            //the state could be broken
            if (principal?.CurrentUserIdentity == null)
                return;

            var selectedOption = this.GetOptionByFilter(optionText);
            if (selectedOption == null)
                throw new InvalidOperationException($"Option was not found for value '{optionText}'");

            if (this.answer == selectedOption.Value)
            {
                this.QuestionState.Validity.ExecutedWithoutExceptions();
                return;
            }

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(new AnswerSingleOptionQuestionCommand(
                    this.interviewId,
                    this.principal.CurrentUserIdentity.UserId,
                    this.Identity.Id,
                    this.Identity.RosterVector,
                    selectedOption.Value)).ConfigureAwait(false);

                this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.answer = selectedOption.Value;
            }
            catch (InterviewException ex)
            {
                this.answer = null;
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private async Task UpdateFilterAndSaveIfExactMatchWithAnyOptionAsync(string filter)
        {
            await this.UpdateFilterAndSuggestionsAsync(filter);

            if (string.IsNullOrEmpty(filter) && this.answer != null)
                await this.RemoveAnswerAsync();
            else
            {
                var selectedOption = this.GetOptionByFilter(filter);
                if (selectedOption != null) await this.SaveAnswerAsync(filter);
            }
            
        }

        protected async Task UpdateFilterAndSuggestionsAsync(string filter)
        {
            this.FilterText = filter;
            this.AutoCompleteSuggestions = await Task.Run(() => this.GetHighlightedSuggestions(filter).ToList());
        }

        private async Task RemoveAnswerAsync()
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.principal.CurrentUserIdentity.UserId,
                        this.Identity));

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        protected async Task SetAnswerAndUpdateFilter()
        {
            var singleOptionQuestion = this.interview.GetSingleOptionQuestion(this.Identity);

            this.answer = singleOptionQuestion.GetAnswer()?.SelectedValue;

            if (!singleOptionQuestion.IsAnswered())
                await this.UpdateFilterAndSuggestionsAsync(string.Empty);
            else
                await this.UpdateFilterAndSuggestionsAsync(this.GetAnsweredOption(this.answer.Value).Title);
        }

        public void Handle(AnswersRemoved @event)
        {
            if (!@event.Questions.Contains(this.Identity)) return;

            this.InvokeOnMainThread(async () => await this.SetAnswerAndUpdateFilter());
        }

        private string RemoveHighlighting(string optionText) => optionText.Replace("</b>", "").Replace("<b>", "");

        private string GetHighlightedText(string text, string filter)
        {
            var startIndexOfSearchedText = string.IsNullOrEmpty(filter)
                ? -1
                : text.IndexOf(filter, StringComparison.OrdinalIgnoreCase);

            return startIndexOfSearchedText >= 0 ? text.Insert(startIndexOfSearchedText + filter.Length, "</b>")
                .Insert(startIndexOfSearchedText, "<b>") : text;
        }

        public virtual void Dispose()
        {
            this.QuestionState.Dispose();
            this.eventRegistry.Unsubscribe(this);
        }
    }
}
