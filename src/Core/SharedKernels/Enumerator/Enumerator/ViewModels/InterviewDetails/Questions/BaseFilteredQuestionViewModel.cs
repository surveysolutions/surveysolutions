using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public abstract class BaseFilteredQuestionViewModel<T> : MvxNotifyPropertyChanged,
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
        private CancellationTokenSource suggestionsCancellation;

        protected Guid interviewId;
        protected IStatefulInterview interview;

        public Identity Identity { get; private set; }

        public IQuestionStateViewModel QuestionState { get; }
        public AnsweringViewModel Answering { get; }
        public QuestionInstructionViewModel InstructionViewModel { get; }

        public T SelectedObject
        {
            set => this.SaveAnswer(value);
        }

        private string filterText;
        public string FilterText
        {
            get => this.filterText;
            set
            {
                this.RaiseAndSetIfChanged(ref this.filterText, value);

                this.UpdateAutoCompleteList(value);
            }
        }

        private List<T> autoCompleteSuggestions = new List<T>();
        public List<T> AutoCompleteSuggestions
        {
            get => this.autoCompleteSuggestions;
            set => this.RaiseAndSetIfChanged(ref this.autoCompleteSuggestions, value);
        }

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);

        protected abstract T ToViewModel(CategoricalOption option, string filter);
        protected abstract IEnumerable<CategoricalOption> GetSuggestions(string filter);
        protected abstract AnswerQuestionCommand CreateAnswerCommand(T answer);
        protected abstract CategoricalOption GetAnsweredOption(int answer);
        protected abstract bool CanSendAnswerCommand(T answer);
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
            this.SetAnswer();

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private async void UpdateAutoCompleteList(string filter)
        {
            this.suggestionsCancellation?.Cancel();
            this.suggestionsCancellation = new CancellationTokenSource();

            await this.UpdateSuggestionsAsync(filter, this.suggestionsCancellation.Token);
        }

        private async Task UpdateSuggestionsAsync(string filter, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            var list = await Task.Run(() => this.GetSuggestionViewModels(filter).ToList());

            if (token.IsCancellationRequested) return;

            this.AutoCompleteSuggestions = list.Any() ? list : new List<T>();
        }

        private IEnumerable<T> GetSuggestionViewModels(string filter)
        {
            foreach (var model in this.GetSuggestions(this.filterText))
            {
                if (model.Title.IsNullOrEmpty())
                    continue;

                yield return this.ToViewModel(model, filter);
            }
        }

        protected virtual async void SaveAnswer(T answer)
        {
            if (answer == null)
            {
                var errorMessage = UIResources.Interview_Question_Filter_MatchError.FormatString(this.FilterText);
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(errorMessage);
                return;
            }

            if (!this.CanSendAnswerCommand(answer)) return;

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(this.CreateAnswerCommand(answer));

                this.FilterText = answer.ToString();

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private async Task RemoveAnswerAsync()
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.principal.CurrentUserIdentity.UserId,
                        this.Identity,
                        DateTime.UtcNow));

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        protected void SetAnswer()
        {
            var singleOptionQuestion = this.interview.GetSingleOptionQuestion(this.Identity);

            if (!singleOptionQuestion.IsAnswered)
                this.FilterText = string.Empty;
            else
            {
                var answer = singleOptionQuestion.GetAnswer().SelectedValue;
                var answerOption = this.GetAnsweredOption(answer);

                this.FilterText = answerOption.Title;
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            if (!@event.Questions.Contains(this.Identity)) return;

            this.InvokeOnMainThread(() => this.FilterText = string.Empty);
        }

        public virtual void Dispose()
        {
            this.QuestionState.Dispose();
            this.eventRegistry.Unsubscribe(this);
        }

        protected static string GetHighlightedText(string text, string filter)
        {
            var startIndexOfSearchedText = string.IsNullOrEmpty(filter)
                ? -1
                : text.IndexOf(filter, StringComparison.OrdinalIgnoreCase);

            return startIndexOfSearchedText >= 0 ? text.Insert(startIndexOfSearchedText + filter.Length, "</b>")
                .Insert(startIndexOfSearchedText, "<b>") : text;
        }
    }
}