using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public abstract class BaseComboboxQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        protected const int SuggestionsMaxCount = 50;
        protected readonly FilteredOptionsViewModel filteredOptionsViewModel;

        protected readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;

        protected CategoricalComboboxAutocompleteViewModel comboboxViewModel;
        protected CovariantObservableCollection<ICompositeEntity> comboboxCollection = new CovariantObservableCollection<ICompositeEntity>();

        protected BaseComboboxQuestionViewModel(
            IPrincipal principal,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            FilteredOptionsViewModel filteredOptionsViewModel)
        {
            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
            this.InstructionViewModel = instructionViewModel;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
        }

        protected Guid interviewId;
        protected IStatefulInterview interview;
        protected int? Answer;

        public Identity Identity { get; private set; }

        public IQuestionStateViewModel QuestionState { get; }
        public AnsweringViewModel Answering { get; }
        public QuestionInstructionViewModel InstructionViewModel { get; }

        
        private List<OptionWithSearchTerm> autoCompleteSuggestions = new List<OptionWithSearchTerm>();
        public List<OptionWithSearchTerm> AutoCompleteSuggestions
        {
            get => this.autoCompleteSuggestions;
            set => this.RaiseAndSetIfChanged(ref this.autoCompleteSuggestions, value);
        }

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);
        
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

            this.optionsTopBorderViewModel = new OptionBorderViewModel(this.QuestionState, true);
            this.optionsBottomBorderViewModel = new OptionBorderViewModel(this.QuestionState, false);

            this.Initialize(interviewId, entityIdentity, navigationState);
            
            this.eventRegistry.Subscribe(this, interviewId);
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

        
        protected async void ComboboxInstantViewModel_OnItemSelected(object sender, int selectedOptionCode)
        {
            await SaveAnswerAsync(selectedOptionCode);
        }

        protected async void ComboboxInstantViewModel_OnAnswerRemoved(object sender, EventArgs e)
        {
            await RemoveAnswerAsync();
        }

        protected async Task RemoveAnswerAsync()
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
        
        public void Handle(AnswersRemoved @event)
        {
            if (!@event.Questions.Contains(this.Identity)) return;

            this.InvokeOnMainThread(async () =>
            {
                if (comboboxViewModel != null) await comboboxViewModel?.UpdateFilter(null);
            });
        }

        public virtual void Dispose()
        {
            this.QuestionState.Dispose();
            this.eventRegistry.Unsubscribe(this);
        }

        protected OptionBorderViewModel optionsTopBorderViewModel;
        protected OptionBorderViewModel optionsBottomBorderViewModel;

        public abstract IObservableCollection<ICompositeEntity> Children { get; }
    }
}
