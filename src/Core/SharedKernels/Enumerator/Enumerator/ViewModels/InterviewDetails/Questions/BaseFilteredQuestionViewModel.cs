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
        protected int? Answer;

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

        private List<OptionWithSearchTerm> autoCompleteSuggestions = new List<OptionWithSearchTerm>();
        public List<OptionWithSearchTerm> AutoCompleteSuggestions
        {
            get => this.autoCompleteSuggestions;
            set => this.RaiseAndSetIfChanged(ref this.autoCompleteSuggestions, value);
        }

        public IMvxAsyncCommand<string> FilterCommand => new MvxAsyncCommand<string>(this.UpdateFilterAndSaveIfExactMatchWithAnyOptionAsync);
        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);
        public IMvxAsyncCommand<OptionWithSearchTerm> SaveAnswerBySelectedOptionCommand => new MvxAsyncCommand<OptionWithSearchTerm>(this.SaveAnswerBySelectedOptionAsync);
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
            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);

            this.Initialize(interviewId, entityIdentity, navigationState);
            Task.Run(this.SetAnswerAndUpdateFilter).WaitAndUnwrapException();

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private IEnumerable<OptionWithSearchTerm> GetFilteredSuggestions(string filter)
        {
            foreach (var model in this.GetSuggestions(filter))
            {
                if (model.Title.IsNullOrEmpty())
                    continue;

                yield return new OptionWithSearchTerm
                {
                    Value = model.Value,
                    Title = model.Title,
                    SearchTerm = filter
                };
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

        private async Task SaveAnswerBySelectedOptionAsync(OptionWithSearchTerm option)
        {
            await this.UpdateFilterAndSuggestionsAsync(option.Title);
            await this.SaveAnswerAsync(option.Value);
        }

        protected virtual async Task SaveAnswerAsync(int optionValue)
        {
            //if app crashed and automatically restored 
            //the state could be broken
            if (principal?.CurrentUserIdentity == null)
                return;

            if (this.Answer == optionValue)
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
                    optionValue)).ConfigureAwait(false);

                this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.Answer = optionValue;
            }
            catch (InterviewException ex)
            {
                this.Answer = null;
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private async Task UpdateFilterAndSaveIfExactMatchWithAnyOptionAsync(string filter)
        {
            await this.UpdateFilterAndSuggestionsAsync(filter);

            if (string.IsNullOrEmpty(filter) && this.Answer != null)
                await this.RemoveAnswerAsync();
            else
            {
                var selectedOption = this.GetOptionByFilter(filter);
                if (selectedOption != null) await this.SaveAnswerAsync(selectedOption.Value);
            }
            
        }

        protected async Task UpdateFilterAndSuggestionsAsync(string filter)
        {
            this.FilterText = filter;
            this.AutoCompleteSuggestions = await Task.Run(() => this.GetFilteredSuggestions(filter).ToList());
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

            this.Answer = singleOptionQuestion.GetAnswer()?.SelectedValue;

            if (!singleOptionQuestion.IsAnswered())
                await this.UpdateFilterAndSuggestionsAsync(string.Empty);
            else
                await this.UpdateFilterAndSuggestionsAsync(this.GetAnsweredOption(this.Answer.Value).Title);
        }

        public void Handle(AnswersRemoved @event)
        {
            if (!@event.Questions.Contains(this.Identity)) return;

            this.InvokeOnMainThread(async () => await this.SetAnswerAndUpdateFilter());
        }

        public virtual void Dispose()
        {
            this.QuestionState.Dispose();
            this.eventRegistry.Unsubscribe(this);
        }
    }
}
