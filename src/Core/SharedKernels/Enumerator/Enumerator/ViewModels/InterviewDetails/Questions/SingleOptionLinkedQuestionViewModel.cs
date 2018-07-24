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
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionLinkedQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<LinkedOptionsChanged>,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly Guid userId;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        protected IStatefulInterview interview;

        public SingleOptionLinkedQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
            this.eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
            this.mainThreadDispatcher = mainThreadDispatcher ?? MvxMainThreadDispatcher.Instance;

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.questionnaireRepository = questionnaireStorage ?? throw new ArgumentNullException("questionnaireStorage");
            this.timer = new Timer(async _ => { await SaveAnswer(); }, null, Timeout.Infinite, Timeout.Infinite);
        }

        private Guid interviewId;

        private Guid linkedToQuestionId;

        private CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel> options;

        private IEnumerable<Guid> parentRosterIds;

        private readonly QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionState;

        private OptionBorderViewModel optionsTopBorderViewModel;

        private OptionBorderViewModel optionsBottomBorderViewModel;

        public CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel> Options
        {
            get => this.options;
            private set { this.options = value; this.RaisePropertyChanged(() => this.HasOptions);}
        }

        public bool HasOptions => this.Options.Any();

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; set; }

        public AnsweringViewModel Answering { get; private set; }

        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity questionIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            this.questionState.Init(interviewId, questionIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, questionIdentity);

            this.interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity, interview.Language);

            this.Identity = questionIdentity;
            this.interviewId = interview.Id;

            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(this.Identity.Id);
            this.parentRosterIds = questionnaire.GetRostersFromTopToSpecifiedEntity(this.linkedToQuestionId).ToHashSet();
            
            this.previousOptionToReset = interview.GetLinkedSingleOptionQuestion(this.Identity).GetAnswer().SelectedValue;

            this.Options = new CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel>(this.CreateOptions());
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
        }

        private IEnumerable<SingleOptionLinkedQuestionOptionViewModel> CreateOptions()
        {
            var linkedQuestion = interview.GetLinkedSingleOptionQuestion(this.Identity);
            
            foreach (var linkedOption in linkedQuestion.Options)
                yield return this.CreateOptionViewModel(linkedOption, linkedQuestion.GetAnswer()?.SelectedValue, interview);
        }

        private async void OptionSelected(object sender, EventArgs eventArgs) => await this.OptionSelectedAsync(sender);

        private async void RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.userId,
                        this.Identity));
                this.QuestionState.Validity.ExecutedWithoutExceptions();
                this.previousOptionToReset = null;
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        private async Task SaveAnswer()
        {
            if (this.selectedOptionToSave.SequenceEqual(this.previousOptionToReset))
                return;

            var selectedOption = this.GetOptionByValue(this.selectedOptionToSave);
            var previousOption = this.GetOptionByValue(this.previousOptionToReset);

            if (selectedOption == null)
                return;

            var command = new AnswerSingleOptionLinkedQuestionCommand(
                this.interviewId,
                this.userId,
                this.Identity.Id,
                this.Identity.RosterVector,
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

        private readonly Timer timer;
        protected internal int ThrottlePeriod { get; set; } = Constants.ThrottlePeriod;
        private decimal[] previousOptionToReset = null;
        private decimal[] selectedOptionToSave = null;

        internal async Task OptionSelectedAsync(object sender)
        {
            var selectedOption = (SingleOptionLinkedQuestionOptionViewModel) sender;
            this.selectedOptionToSave = selectedOption.RosterVector;

            this.Options.Where(option => option.Selected && option != selectedOption).ForEach(x => x.Selected = false);

            if (this.ThrottlePeriod == 0)
            {
                await SaveAnswer();
            }
            else
            {
                timer.Change(ThrottlePeriod, Timeout.Infinite);
            }
        }

        private SingleOptionLinkedQuestionOptionViewModel GetOptionByValue(decimal[] value)
        {
            return value != null 
                ? this.Options.FirstOrDefault(x => Enumerable.SequenceEqual(x.RosterVector, value))
                : null;
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.Identity.Equals(question.Id, question.RosterVector))
                {
                    foreach (var option in this.Options.Where(option => option.Selected))
                    {
                        option.Selected = false;
                    }

                    this.previousOptionToReset = null;
                }
            }
        }

        public void Handle(LinkedOptionsChanged @event)
        {
            ChangedLinkedOptions changedLinkedQuestion = @event.ChangedLinkedQuestions.SingleOrDefault(x => x.QuestionId == this.Identity);

            if (changedLinkedQuestion != null)
            {
                this.RefreshOptionsFromModel();
            }
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var optionListShouldBeUpdated = @event.ChangedInstances.Any(x => this.parentRosterIds.Contains(x.RosterInstance.GroupId));
            if (optionListShouldBeUpdated)
            {
                this.RefreshOptionsFromModel();
            }
        }

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                this.optionsTopBorderViewModel =
                    new OptionBorderViewModel(this.questionState, true)
                    {
                        HasOptions = HasOptions
                    };
                result.Add(this.optionsTopBorderViewModel);
                result.AddCollection(this.Options);
                this.optionsBottomBorderViewModel =
                    new OptionBorderViewModel(this.questionState, false)
                    {
                        HasOptions = HasOptions
                    };
                result.Add(this.optionsBottomBorderViewModel);
                return result;
            }
        }

        private void RefreshOptionsFromModel()
        {
            this.mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                var newOptions = this.CreateOptions();
                var removedItems = this.Options.SynchronizeWith(newOptions.ToList(), (s, t) => s.RosterVector.Identical(t.RosterVector) && s.Title == t.Title);
                removedItems.ForEach(option =>
                {
                    option.BeforeSelected -= this.OptionSelected;
                    option.AnswerRemoved -= this.RemoveAnswer;
                });

                this.RaisePropertyChanged(() => this.HasOptions);
            });
        }

        private SingleOptionLinkedQuestionOptionViewModel CreateOptionViewModel(RosterVector linkedOption, RosterVector answeredOption, IStatefulInterview interview)
        {
            var optionViewModel = new SingleOptionLinkedQuestionOptionViewModel
            {
                Enablement = this.questionState.Enablement,
                RosterVector = linkedOption,
                Title = interview.GetLinkedOptionTitle(this.Identity, linkedOption),
                Selected = linkedOption.Equals(answeredOption),
                QuestionState = this.questionState,
            };

            optionViewModel.BeforeSelected += this.OptionSelected;
            optionViewModel.AnswerRemoved += this.RemoveAnswer;

            return optionViewModel;
        }
    }
}
