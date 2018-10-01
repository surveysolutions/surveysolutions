using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
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
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public abstract class MultiOptionLinkedQuestionBaseViewModel : MvxNotifyPropertyChanged,
        IMultiOptionQuestionViewModelToggleable,
        ILiteEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        ILiteEventHandler<AnswersRemoved>,
        IInterviewEntityViewModel,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPrincipal userIdentity;
        protected readonly ILiteEventRegistry eventRegistry;
        protected readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        protected int? maxAllowedAnswers;
        protected Guid interviewId;
        protected IStatefulInterview interview;
        protected Guid userId;
        protected Identity questionIdentity;
        protected bool areAnswersOrdered;
        protected readonly QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState;
        private OptionBorderViewModel optionsTopBorderViewModel;
        private OptionBorderViewModel optionsBottomBorderViewModel;

        public IQuestionStateViewModel QuestionState => this.questionState;

        public AnsweringViewModel Answering { get; protected set; }
        public QuestionInstructionViewModel InstructionViewModel { get; set; }

        protected MultiOptionLinkedQuestionBaseViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IPrincipal userIdentity,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher)
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
            this.timer = new Timer(async _ => { await SaveAnswer(); }, null, Timeout.Infinite, Timeout.Infinite);
        }

        public Identity Identity => this.questionIdentity;
        public bool AreAnswersOrdered => this.areAnswersOrdered;

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

            PreviousOptionsToReset = interview.GetLinkedMultiOptionQuestion(this.Identity)?.GetAnswer()?.CheckedValues?.ToList();
        }
        protected abstract void InitFromModel(IQuestionnaire questionnaire);
        protected abstract IEnumerable<MultiOptionLinkedQuestionOptionViewModel> CreateOptions();

        public virtual void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
        }

        public CovariantObservableCollection<MultiOptionLinkedQuestionOptionViewModel> Options { get; }
        public Identity QuestionIdentity => this.Identity;
        IObservableCollection<MultiOptionQuestionOptionViewModelBase> IMultiOptionQuestionViewModelToggleable.Options => this.Options;
        public bool HasOptions => this.Options.Any();

        private readonly Timer timer;
        protected internal int ThrottlePeriod { get; set; } = Constants.ThrottlePeriod;
        private List<RosterVector> previousOptionsToReset = null;
        private List<RosterVector> PreviousOptionsToReset
        {
            get => previousOptionsToReset ?? new List<RosterVector>();
            set => this.previousOptionsToReset = value;
        }

        private List<RosterVector> selectedOptionsToSave = null;

        private async Task SaveAnswer()
        {
            var command = new AnswerMultipleOptionsLinkedQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                selectedOptionsToSave.ToArray());

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                
                if (selectedOptionsToSave.Count == this.maxAllowedAnswers)
                {
                    this.Options.Where(o => !o.Checked).ForEach(o => o.CanBeChecked = false);
                }
                else
                {
                    this.Options.ForEach(x => x.CanBeChecked = true);
                }

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                ResetUiOptions();
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public async Task ToggleAnswerAsync(MultiOptionQuestionOptionViewModelBase changedModel)
        {
            List<MultiOptionLinkedQuestionOptionViewModel> allSelectedOptions =
                this.areAnswersOrdered ?
                    this.Options.Where(x => x.Checked).OrderBy(x => x.CheckedOrder).ToList() :
                    this.Options.Where(x => x.Checked).ToList();

            if (this.maxAllowedAnswers.HasValue && allSelectedOptions.Count > this.maxAllowedAnswers)
            {
                changedModel.Checked = false;
                return;
            }

            selectedOptionsToSave = allSelectedOptions
                .Select(x => new RosterVector(x.Value))
                .ToList();

            if (this.ThrottlePeriod == 0)
            {
                await SaveAnswer();
            }
            else
            {
                timer.Change(ThrottlePeriod, Timeout.Infinite);
            }
        }

        private void ResetUiOptions()
        {
            var interview = this.interviewRepository.Get(this.interviewId.FormatGuid());
            var answer = interview.GetLinkedMultiOptionQuestion(this.Identity)?.GetAnswer();
            if (answer == null) return;
            var checkedOptions = answer.CheckedValues.ToArray();
            foreach (var option in Options)
            {
                var selectedOptionIndex = Array.IndexOf(checkedOptions, option.Value);
                option.Checked = selectedOptionIndex >= 0;
                if (this.areAnswersOrdered)
                {
                    option.CheckedOrder = selectedOptionIndex + 1;
                }
            }

            PreviousOptionsToReset = answer.CheckedValues.ToList();
        }

        private void PutOrderOnOptions(MultipleOptionsLinkedQuestionAnswered @event)
        {
            var moreOptionsCanBeChecked = !this.maxAllowedAnswers.HasValue || @event.SelectedRosterVectors.Length < this.maxAllowedAnswers;

            foreach (var option in this.Options)
            {
                var foundIndex = @event.SelectedRosterVectors
                    .Select((o, i) => new { index = i, selectedVector = o })
                    .FirstOrDefault(item => item.selectedVector.Identical(option.Value));

                var selectedOptionIndex = foundIndex?.index ?? -1;

                if (selectedOptionIndex >= 0)
                {
                    option.Checked = true;
                    option.CheckedOrder = selectedOptionIndex + 1;
                }
                else
                {
                    option.Checked = false;
                    option.CheckedOrder = null;
                    option.CanBeChecked = moreOptionsCanBeChecked;
                }
            }
        }


        protected void UpdateMaxAnswersCountMessage(int answersCount)
        {
            if (this.maxAllowedAnswers.HasValue && this.HasOptions)
            {
                this.MaxAnswersCountMessage = string.Format(UIResources.Interview_MaxAnswersCount, 
                    answersCount, Math.Min(this.maxAllowedAnswers.Value, this.Options.Count));
            }
        }

        private string maxAnswersCountMessage;
        public string MaxAnswersCountMessage
        {
            get => maxAnswersCountMessage;
            set => SetProperty(ref maxAnswersCountMessage, value);
        }

        public void Handle(MultipleOptionsLinkedQuestionAnswered @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id && @event.RosterVector.Identical(this.questionIdentity.RosterVector))
            {
                if (this.areAnswersOrdered)
                {
                    this.PutOrderOnOptions(@event);
                }
                UpdateMaxAnswersCountMessage(@event.SelectedRosterVectors?.Length ?? 0);
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            if (@event.Questions.Any(x => x.Id == this.questionIdentity.Id && x.RosterVector.Identical(this.questionIdentity.RosterVector)))
            {
                foreach (var option in this.Options)
                {
                    option.Checked = false;
                    option.CheckedOrder = null;
                    option.CanBeChecked = true;
                }

                PreviousOptionsToReset = null;
                UpdateMaxAnswersCountMessage(0);
            }
        }

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                this.optionsTopBorderViewModel = new OptionBorderViewModel(this.questionState, true)
                {
                    HasOptions = HasOptions
                };
                result.Add(this.optionsTopBorderViewModel);
                result.AddCollection(this.Options);
                this.optionsBottomBorderViewModel = new OptionBorderViewModel(this.questionState, false)
                {
                    HasOptions = HasOptions
                };
                result.Add(this.optionsBottomBorderViewModel);
                return result;
            }
        }
    }
}
