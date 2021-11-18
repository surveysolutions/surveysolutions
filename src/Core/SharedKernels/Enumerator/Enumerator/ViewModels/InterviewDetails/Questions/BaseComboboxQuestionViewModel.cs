using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public abstract class BaseComboboxQuestionViewModel : InterviewQuestionViewModelBase,
        IInterviewEntityViewModel,
        IViewModelEventHandler<AnswersRemoved>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        protected const int SuggestionsMaxCount = 50;
        protected readonly FilteredOptionsViewModel filteredOptionsViewModel;

        protected readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry eventRegistry;

        protected readonly CategoricalComboboxAutocompleteViewModel comboboxViewModel;
        protected CovariantObservableCollection<ICompositeEntity> comboboxCollection = new CovariantObservableCollection<ICompositeEntity>();

        protected BaseComboboxQuestionViewModel(
            IPrincipal principal,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            IViewModelEventRegistry eventRegistry,
            FilteredOptionsViewModel filteredOptionsViewModel)
        {
            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.questionState = questionStateViewModel;
            this.Answering = answering;
            this.InstructionViewModel = instructionViewModel;
            this.filteredOptionsViewModel = filteredOptionsViewModel;

            this.optionsTopBorderViewModel = new OptionBorderViewModel(this.QuestionState, true);
            this.optionsBottomBorderViewModel = new OptionBorderViewModel(this.QuestionState, false);
            
            this.comboboxViewModel = 
                new CategoricalComboboxAutocompleteViewModel(questionStateViewModel, filteredOptionsViewModel, 
                    true);
        }

        protected IStatefulInterview interview;
        protected int? Answer;

        private readonly QuestionStateViewModel<SingleOptionQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;
        public AnsweringViewModel Answering { get; }
        public QuestionInstructionViewModel InstructionViewModel { get; }
        
        public override void InitFast()
        {
            this.interview = this.interviewRepository.GetOrThrow(interviewId);

            this.questionState.Init(interviewId, questionIdentity, NavigationState);
            this.InstructionViewModel.Init(interviewId, questionIdentity, NavigationState);
        }

        public override void InitData()
        {
            this.filteredOptionsViewModel.Init(interviewId, questionIdentity, SuggestionsMaxCount);

            this.Answer = GetCurrentAnswer();
            var initialFilter = this.Answer.HasValue ? this.filteredOptionsViewModel.GetAnsweredOption(this.Answer.Value)?.Title ?? null : null;

            this.comboboxViewModel.Init(interviewId, questionIdentity, NavigationState);
            this.comboboxViewModel.InitFilter(initialFilter);
            this.comboboxViewModel.OnItemSelected += ComboboxInstantViewModel_OnItemSelected;
            this.comboboxViewModel.OnAnswerRemoved += ComboboxInstantViewModel_OnAnswerRemoved;
            this.comboboxViewModel.OnShowErrorIfNoAnswer += ComboboxViewModel_OnShowErrorIfNoAnswer;

            comboboxCollection.Add(comboboxViewModel);

            this.eventRegistry.Subscribe(this, interviewId);        }

        protected virtual int? GetCurrentAnswer()
        {
            return this.interview.GetSingleOptionQuestion(this.Identity).GetAnswer()?.SelectedValue;
        }

        public virtual async Task SaveAnswerAsync(int optionValue)
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
                    interview.Id,
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
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interview.Id,
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

            this.Answer = null;

            comboboxViewModel?.ResetFilterAndOptions();
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
        }

        protected OptionBorderViewModel optionsTopBorderViewModel;
        protected OptionBorderViewModel optionsBottomBorderViewModel;

        public abstract IObservableCollection<ICompositeEntity> Children { get; }
    }
}
