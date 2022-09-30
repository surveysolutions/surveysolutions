using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public abstract class BaseComboboxQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IAsyncViewModelEventHandler<AnswersRemoved>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
		//this value determines a number of suggestions shown in combo-box
        protected const int SuggestionsMaxCount = 50;
        protected readonly FilteredOptionsViewModel filteredOptionsViewModel;
        protected readonly IInterviewViewModelFactory viewModelFactory;
        protected readonly IPrincipal principal;
        protected readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry eventRegistry;

        protected CategoricalComboboxAutocompleteViewModel comboboxViewModel;
        protected CovariantObservableCollection<ICompositeEntity> comboboxCollection = new CovariantObservableCollection<ICompositeEntity>();

        protected BaseComboboxQuestionViewModel(
            IPrincipal principal,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            IViewModelEventRegistry eventRegistry,
            FilteredOptionsViewModel filteredOptionsViewModel,
            IInterviewViewModelFactory viewModelFactory)
        {
            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));

            this.questionState = questionStateViewModel;
            this.Answering = answering;
            this.InstructionViewModel = instructionViewModel;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.viewModelFactory = viewModelFactory;

            this.optionsTopBorderViewModel = new OptionBorderViewModel(this.QuestionState, true);
            this.optionsBottomBorderViewModel = new OptionBorderViewModel(this.QuestionState, false);
        }

        protected string interviewId;
        protected int? Answer;

        public Identity Identity { get; private set; }

        private readonly QuestionStateViewModel<SingleOptionQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;
        public AnsweringViewModel Answering { get; }
        public QuestionInstructionViewModel InstructionViewModel { get; }
        
        public virtual void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.Identity = entityIdentity;
            this.interviewId = interviewId;

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);

            this.filteredOptionsViewModel.Init(interviewId, entityIdentity, SuggestionsMaxCount);

            this.Answer = GetCurrentAnswer();
            var initialFilter = this.Answer.HasValue ? this.filteredOptionsViewModel.GetAnsweredOption(this.Answer.Value)?.Title ?? null : null;

            this.comboboxViewModel.Init(interviewId, entityIdentity, navigationState);
            this.comboboxViewModel.InitFilter(initialFilter);
            this.comboboxViewModel.OnItemSelected += ComboboxInstantViewModel_OnItemSelected;
            this.comboboxViewModel.OnAnswerRemoved += ComboboxInstantViewModel_OnAnswerRemoved;
            this.comboboxViewModel.OnShowErrorIfNoAnswer += ComboboxViewModel_OnShowErrorIfNoAnswer;

            comboboxCollection.Add(comboboxViewModel);

            this.ChangeAttachment(Answer);

            this.eventRegistry.Subscribe(this, interviewId);
        }

        protected virtual int? GetCurrentAnswer()
        {
            var interview = this.interviewRepository.GetOrThrow(this.interviewId);
            return interview.GetSingleOptionQuestion(this.Identity).GetAnswer()?.SelectedValue;
        }

        public virtual async Task SaveAnswerAsync(int optionValue)
        {
            //if app crashed and automatically restored 
            //the state could be broken
            if (principal?.CurrentUserIdentity == null)
                return;

            if (this.Answer == optionValue)
            {
                await this.QuestionState.Validity.ExecutedWithoutExceptions();
                return;
            }

            try
            {
                await this.Answering.SendQuestionCommandAsync(new AnswerSingleOptionQuestionCommand(
                    Guid.Parse(this.interviewId),
                    this.principal.CurrentUserIdentity.UserId,
                    this.Identity.Id,
                    this.Identity.RosterVector,
                    optionValue)).ConfigureAwait(false);

                await this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.Answer = optionValue;

                ChangeAttachment(optionValue);
            }
            catch (InterviewException ex)
            {
                this.Answer = null;
                await this.QuestionState.Validity.ProcessException(ex);
            }
        }

        protected abstract void ChangeAttachment(int? optionValue);


        protected async Task ComboboxInstantViewModel_OnItemSelected(object sender, int selectedOptionCode)
        {
            await SaveAnswerAsync(selectedOptionCode);
        }

        protected async Task ComboboxInstantViewModel_OnAnswerRemoved(object sender, EventArgs e)
        {
            await RemoveAnswerAsync();
        }

        private async Task ComboboxViewModel_OnShowErrorIfNoAnswer(object sender, EventArgs e)
        {
            if (this.comboboxViewModel.FilterText == string.Empty && this.questionState.IsAnswered)
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(string.Format(UIResources.Interview_Question_Filter_MatchError, string.Empty));
        }

        protected async Task RemoveAnswerAsync()
        {
            try
            {
                await this.Answering.SendQuestionCommandAsync(
                    new RemoveAnswerCommand(Guid.Parse(this.interviewId),
                        this.principal.CurrentUserIdentity.UserId,
                        this.Identity));

                await this.QuestionState.Validity.ExecutedWithoutExceptions();

                ChangeAttachment(null);
            }
            catch (InterviewException exception)
            {
                await this.QuestionState.Validity.ProcessException(exception);
            }
        }
        
        public virtual async Task HandleAsync(AnswersRemoved @event)
        {
            if (!@event.Questions.Contains(this.Identity)) return;

            this.Answer = null;

            if(comboboxViewModel!= null)
                await comboboxViewModel.ResetFilterAndOptions();
        }

        public virtual void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);

            this.comboboxViewModel.OnItemSelected -= ComboboxInstantViewModel_OnItemSelected;
            this.comboboxViewModel.OnAnswerRemoved -= ComboboxInstantViewModel_OnAnswerRemoved;
            this.comboboxViewModel.OnShowErrorIfNoAnswer -= ComboboxViewModel_OnShowErrorIfNoAnswer;
            this.comboboxViewModel.Dispose();

            this.QuestionState.Dispose();
            this.InstructionViewModel.Dispose();
            this.filteredOptionsViewModel.Dispose();
        }

        protected OptionBorderViewModel optionsTopBorderViewModel;
        protected OptionBorderViewModel optionsBottomBorderViewModel;

        public abstract IObservableCollection<ICompositeEntity> Children { get; }
    }
}
