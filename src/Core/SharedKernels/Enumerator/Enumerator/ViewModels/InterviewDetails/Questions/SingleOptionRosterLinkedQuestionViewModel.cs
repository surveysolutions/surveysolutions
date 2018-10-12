using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
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
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionRosterLinkedQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        ILiteEventHandler<LinkedOptionsChanged>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        private readonly ThrottlingViewModel throttlingModel;

        public SingleOptionRosterLinkedQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering, 
            ThrottlingViewModel throttlingModel)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));
            if (eventRegistry == null) throw new ArgumentNullException("eventRegistry");

            this.userId = principal.CurrentUserIdentity.UserId;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher ?? Mvx.Resolve<IMvxMainThreadAsyncDispatcher>();

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.throttlingModel = throttlingModel;
            this.throttlingModel.Init(SaveAnswer);
        }

        private Identity questionIdentity;
        private Guid interviewId;
        private IStatefulInterview interview;
        private Guid linkedToRosterId;
        private CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel> options;
        private HashSet<Guid> parentRosters;
        private readonly QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionState;
        private OptionBorderViewModel optionsTopBorderViewModel;
        private OptionBorderViewModel optionsBottomBorderViewModel;

        public CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel> Options
        {
            get => this.options;
            private set
            {
                this.options = value;
                this.RaisePropertyChanged(() => this.HasOptions);
            }
        }

        public bool HasOptions => this.Options.Any();

        public IQuestionStateViewModel QuestionState => this.questionState;

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

        public QuestionInstructionViewModel InstructionViewModel { get; set; }
        public AnsweringViewModel Answering { get; private set; }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity questionIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            this.questionState.Init(interviewId, questionIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, questionIdentity);

            interview = this.interviewRepository.Get(interviewId);

            this.questionIdentity = questionIdentity;
            this.interviewId = interview.Id;

            var questionnaire =
                this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity,
                    this.interview.Language);
            this.linkedToRosterId = questionnaire.GetRosterReferencedByLinkedQuestion(questionIdentity.Id);
            this.parentRosters = questionnaire.GetRostersFromTopToSpecifiedEntity(this.linkedToRosterId).ToHashSet();
            this.Options =
                new CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel>(this.CreateOptions());

            var question = interview.GetLinkedSingleOptionQuestion(this.Identity);
            this.previousOptionToReset = question.IsAnswered() ? (decimal[])question.GetAnswer()?.SelectedValue : (decimal[])null;

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
            this.eventRegistry.Subscribe(this, interviewId);
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();

            foreach (var option in Options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
            }
            this.throttlingModel.Dispose();
        }

        private IEnumerable<SingleOptionLinkedQuestionOptionViewModel> CreateOptions()
        {
            var linkedQuestion = interview.GetLinkedSingleOptionQuestion(this.Identity);

            foreach (var linkedOption in linkedQuestion.Options)
                yield return this.CreateOptionViewModel(linkedOption, linkedQuestion.GetAnswer()?.SelectedValue,
                    interview);
        }

        private async void OptionSelected(object sender, EventArgs eventArgs)
        {
            await this.OptionSelectedAsync(sender);
        }

        private readonly Timer timer;

        protected internal int ThrottlePeriod { get; set; } = Constants.ThrottlePeriod;

        private decimal[] previousOptionToReset = null;

        private decimal[] selectedOptionToSave = null;

        private async Task SaveAnswer()
        {
            if (this.previousOptionToReset != null && this.selectedOptionToSave.SequenceEqual(this.previousOptionToReset))
                return;

            var selectedOption = this.GetOptionByValue(this.selectedOptionToSave);
            var previousOption = this.GetOptionByValue(this.previousOptionToReset);

            if (selectedOption == null)
                return;

            var command = new AnswerSingleOptionLinkedQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                selectedOption.RosterVector);

            try
            {
                if (previousOption != null)
                {
                    previousOption.Selected = false;
                }

                await this.Answering.SendAnswerQuestionCommandAsync(command);

                this.previousOptionToReset = this.selectedOptionToSave;

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                selectedOption.Selected = false;

                if (previousOption != null)
                {
                    previousOption.Selected = true;
                }

                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private SingleOptionLinkedQuestionOptionViewModel GetOptionByValue(decimal[] value)
        {
            return value != null
                ? this.Options.FirstOrDefault(x => Enumerable.SequenceEqual(x.RosterVector, value))
                : null;
        }

        internal async Task OptionSelectedAsync(object sender)
        {
            var selectedOption = (SingleOptionLinkedQuestionOptionViewModel) sender;
            this.selectedOptionToSave = selectedOption.RosterVector;

            this.Options.Where(x => x.Selected && x != selectedOption).ForEach(x => x.Selected = false);

            if (this.ThrottlePeriod == 0)
            {
                await SaveAnswer();
            }
            else
            {
                timer.Change(ThrottlePeriod, Timeout.Infinite);
            }
        }

        private async void RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.userId,
                        this.questionIdentity));
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                foreach (var option in this.Options.Where(option => option.Selected).ToList())
                {
                    option.Selected = false;
                }

                this.previousOptionToReset = null;
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    foreach (var option in this.Options.Where(option => option.Selected))
                    {
                        option.Selected = false;
                    }

                    this.previousOptionToReset = null;
                }
            }
        }

        private SingleOptionLinkedQuestionOptionViewModel CreateOptionViewModel(RosterVector linkedOption,
            RosterVector answeredOption, IStatefulInterview interview)
        {
            var optionViewModel = new SingleOptionLinkedQuestionOptionViewModel
            {
                RosterVector = linkedOption,
                Title = interview.GetLinkedOptionTitle(this.Identity, linkedOption),
                Selected = linkedOption.Equals(answeredOption),
                QuestionState = this.questionState
            };

            optionViewModel.BeforeSelected += this.OptionSelected;
            optionViewModel.AnswerRemoved += this.RemoveAnswer;

            return optionViewModel;
        }

        public async void Handle(RosterInstancesTitleChanged @event)
        {
            var optionListShouldBeUpdated = @event.ChangedInstances.Any(x =>
                x.RosterInstance.GroupId == this.linkedToRosterId ||
                this.parentRosters.Contains(x.RosterInstance.GroupId));
            if (optionListShouldBeUpdated)
            {
                await this.RefreshOptionsListFromModelAsync();
            }
        }

        public async void Handle(LinkedOptionsChanged @event)
        {
            var optionListShouldBeUpdated = @event.ChangedLinkedQuestions.Any(x => x.QuestionId.Id == this.Identity.Id);
            if (optionListShouldBeUpdated)
            {
                await this.RefreshOptionsListFromModelAsync();
            }
        }

        private async Task RefreshOptionsListFromModelAsync()
        {
            var optionsToUpdate = this.CreateOptions().ToArray();

            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.Options.SynchronizeWith(optionsToUpdate,
                    (s, t) => s.RosterVector.Identical(t.RosterVector) && s.Title == t.Title);
                this.RaisePropertyChanged(() => this.HasOptions);
            });
        }
    }
}
