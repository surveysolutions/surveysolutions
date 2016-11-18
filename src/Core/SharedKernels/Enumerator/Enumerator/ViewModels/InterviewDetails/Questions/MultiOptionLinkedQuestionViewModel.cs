using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public abstract class MultiOptionLinkedQuestionViewModel : MvxNotifyPropertyChanged,
        IMultiOptionQuestionViewModelToggleable,
        ILiteEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IInterviewEntityViewModel,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPrincipal userIdentity;
        protected readonly ILiteEventRegistry eventRegistry;
        protected readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        protected int? maxAllowedAnswers;
        protected Guid interviewId;
        protected IStatefulInterview interview;
        protected Guid userId;
        protected Identity questionIdentity;
        protected bool areAnswersOrdered;
        protected readonly QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState;
        private OptionBorderViewModel<MultipleOptionsLinkedQuestionAnswered> optionsTopBorderViewModel;
        private OptionBorderViewModel<MultipleOptionsLinkedQuestionAnswered> optionsBottomBorderViewModel;

        public IQuestionStateViewModel QuestionState => this.questionState;

        public AnsweringViewModel Answering { get; protected set; }
        public QuestionInstructionViewModel InstructionViewModel { get; set; }

        protected MultiOptionLinkedQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IPrincipal userIdentity,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.userIdentity = userIdentity;
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.questionState = questionState;
            this.Answering = answering;
            this.InstructionViewModel = instructionViewModel;
            this.Options = new CovariantObservableCollection<MultiOptionLinkedQuestionOptionViewModel>();
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.eventRegistry.Subscribe(this, interviewId);
            this.questionIdentity = entityIdentity;
            this.userId = this.userIdentity.CurrentUserIdentity.UserId;
            this.interview = this.interviewRepository.Get(interviewId);
            this.interviewId = this.interview.Id;
            this.InitFromModel(this.questionnaireStorage.GetQuestionnaire(this.interview.QuestionnaireIdentity, this.interview.Language));

            this.InstructionViewModel.Init(interviewId, entityIdentity);
            var childViewModels = this.CreateOptions();

            this.Options.Clear();
            this.Options.CollectionChanged += (sender, args) =>
            {
                if (this.optionsTopBorderViewModel != null)
                {
                    this.optionsTopBorderViewModel.HasOptions = HasOptions;
                }
                if (this.optionsBottomBorderViewModel != null)
                {
                    this.optionsBottomBorderViewModel.HasOptions = this.HasOptions;
                }
            };
            
            childViewModels.ForEach(x => this.Options.Add(x));
        }
        protected abstract void InitFromModel(IQuestionnaire questionnaire);
        protected abstract IEnumerable<MultiOptionLinkedQuestionOptionViewModel> CreateOptions();

        public virtual void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
        }

        public CovariantObservableCollection<MultiOptionLinkedQuestionOptionViewModel> Options { get; }

        public bool HasOptions => this.Options.Any();

        public async Task ToggleAnswerAsync(MultiOptionQuestionOptionViewModelBase changedModel)
        {
            List<MultiOptionLinkedQuestionOptionViewModel> allSelectedOptions =
                this.areAnswersOrdered ?
                    this.Options.Where(x => x.Checked).OrderBy(x => x.CheckedTimeStamp).ThenBy(x => x.CheckedOrder).ToList() :
                    this.Options.Where(x => x.Checked).ToList();

            if (this.maxAllowedAnswers.HasValue && allSelectedOptions.Count > this.maxAllowedAnswers)
            {
                changedModel.Checked = false;
                return;
            }

            decimal[][] selectedValues = allSelectedOptions
                .Select(x => x.Value)
                .ToArray();

            var command = new AnswerMultipleOptionsLinkedQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                selectedValues);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                changedModel.Checked = !changedModel.Checked;
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void PutOrderOnOptions(MultipleOptionsLinkedQuestionAnswered @event)
        {
            foreach (var option in this.Options)
            {
                var foundIndex = @event.SelectedRosterVectors
                    .Select((o, i) => new { index = i, selectedVector = o })
                    .FirstOrDefault(item => item.selectedVector.Identical(option.Value));

                var selectedOptionIndex = foundIndex?.index ?? -1;

                if (selectedOptionIndex >= 0)
                {
                    option.CheckedOrder = selectedOptionIndex + 1;
                    option.Checked = true;
                }
                else
                {
                    option.CheckedOrder = null;
                    option.Checked = false;
                }
            }
        }

        public void Handle(MultipleOptionsLinkedQuestionAnswered @event)
        {
            if (this.areAnswersOrdered && @event.QuestionId == this.questionIdentity.Id && @event.RosterVector.Identical(this.questionIdentity.RosterVector))
            {
                this.PutOrderOnOptions(@event);
            }
        }

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                this.optionsTopBorderViewModel = new OptionBorderViewModel<MultipleOptionsLinkedQuestionAnswered>(this.questionState, true)
                {
                    HasOptions = HasOptions
                };
                result.Add(this.optionsTopBorderViewModel);
                result.AddCollection(this.Options);
                this.optionsBottomBorderViewModel = new OptionBorderViewModel<MultipleOptionsLinkedQuestionAnswered>(this.questionState, false)
                {
                    HasOptions = HasOptions
                };
                result.Add(this.optionsBottomBorderViewModel);
                return result;
            }
        }

    }
}