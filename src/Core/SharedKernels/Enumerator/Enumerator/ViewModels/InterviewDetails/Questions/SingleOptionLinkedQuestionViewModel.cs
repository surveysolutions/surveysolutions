#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionLinkedQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        IViewModelEventHandler<AnswersRemoved>,
        IAsyncViewModelEventHandler<LinkedOptionsChanged>,
        IAsyncViewModelEventHandler<RosterInstancesTitleChanged>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly Guid userId;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        private readonly ThrottlingViewModel throttlingModel;

        public SingleOptionLinkedQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository interviewRepository,
            IViewModelEventRegistry eventRegistry,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering, 
            ThrottlingViewModel throttlingModel,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
            this.eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
            
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.throttlingModel = throttlingModel;
            this.questionnaireRepository = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.throttlingModel.Init(SaveAnswer);
            this.options = new CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel>();
            this.parentRosterIds = Enumerable.Empty<Guid>();
        }

        private string interviewId = null!;

        private Guid linkedToQuestionId;

        private CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel> options;

        private IEnumerable<Guid> parentRosterIds;

        private readonly QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionState;

        private OptionBorderViewModel? optionsTopBorderViewModel;

        private OptionBorderViewModel? optionsBottomBorderViewModel;

        public CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel> Options
        {
            get => this.options;
            private set
            {
                this.options = value; 
                this.RaisePropertyChanged(nameof(this.HasOptions));
            }
        }

        public bool HasOptions => this.Options.Any();

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public AnsweringViewModel Answering { get; }

        public Identity Identity { get; private set; } = null!;

        public void Init(string interviewId, Identity questionIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            this.questionState.Init(interviewId, questionIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, questionIdentity, navigationState);

            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);

            this.Identity = questionIdentity;
            this.interviewId = interviewId;

            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(this.Identity.Id);
            this.parentRosterIds = questionnaire.GetRostersFromTopToSpecifiedEntity(this.linkedToQuestionId).ToHashSet();

            var question = interview.GetLinkedSingleOptionQuestion(this.Identity);
            this.previousOptionToReset = question.IsAnswered() ? (decimal[])question.GetAnswer()?.SelectedValue : null;

            this.Options = new CovariantObservableCollection<SingleOptionLinkedQuestionOptionViewModel>(this.CreateOptions());
            this.Options.CollectionChanged += CollectionChanged;

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.optionsTopBorderViewModel != null)
            {
                this.optionsTopBorderViewModel.HasOptions = HasOptions;
            }

            if (this.optionsBottomBorderViewModel != null)
            {
                this.optionsBottomBorderViewModel.HasOptions = this.HasOptions;
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
            this.InstructionViewModel.Dispose();

            foreach (var option in Options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
            }
            this.Options.CollectionChanged -= CollectionChanged;
            
            this.throttlingModel.Dispose();
        }

        private IEnumerable<SingleOptionLinkedQuestionOptionViewModel> CreateOptions()
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var linkedQuestion = interview.GetLinkedSingleOptionQuestion(this.Identity);
            
            foreach (var linkedOption in linkedQuestion.Options)
                yield return this.CreateOptionViewModel(linkedOption, linkedQuestion.GetAnswer()?.SelectedValue, interview);
        }

        private async void OptionSelected(object? sender, EventArgs eventArgs) => await this.OptionSelectedAsync(sender);

        private async Task SaveAnswer()
        {
            if (this.previousOptionToReset != null && this.selectedOptionToSave.SequenceEqual(this.previousOptionToReset))
                return;

            var selectedOption = this.GetOptionByValue(this.selectedOptionToSave);
            var previousOption = this.GetOptionByValue(this.previousOptionToReset);

            if (selectedOption == null)
                return;

            var command = new AnswerSingleOptionLinkedQuestionCommand(
                Guid.Parse(this.interviewId),
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

                await this.Answering.SendQuestionCommandAsync(command);
                
                this.previousOptionToReset = this.selectedOptionToSave;

                await this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                selectedOption.Selected = false;

                if (previousOption != null)
                {
                    previousOption.Selected = true;
                }

                await this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private decimal[]? previousOptionToReset = null;

        private decimal[]? selectedOptionToSave = null;

        internal async Task OptionSelectedAsync(object? sender)
        {
            if (sender == null) return;
            var selectedOption = (SingleOptionLinkedQuestionOptionViewModel) sender;
            this.selectedOptionToSave = selectedOption.RosterVector;

            this.Options.Where(option => option.Selected && option != selectedOption).ForEach(x => x.Selected = false);

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        private SingleOptionLinkedQuestionOptionViewModel? GetOptionByValue(decimal[]? value)
        {
            return value != null 
                ? this.Options.FirstOrDefault(x => Enumerable.SequenceEqual(x.RosterVector, value))
                : null;
        }

        private async void RemoveAnswer(object? sender, EventArgs e)
        {
            try
            {
                this.throttlingModel.CancelPendingAction();
                
                await this.Answering.SendQuestionCommandAsync(
                    new RemoveAnswerCommand(Guid.Parse(this.interviewId),
                        this.userId,
                        this.Identity));
                await this.QuestionState.Validity.ExecutedWithoutExceptions();

                foreach (var option in this.Options.Where(option => option.Selected).ToList())
                {
                    option.Selected = false;
                }

                this.previousOptionToReset = null;
            }
            catch (InterviewException exception)
            {
                await this.QuestionState.Validity.ProcessException(exception);
            }
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

        public async Task HandleAsync(LinkedOptionsChanged @event)
        {
            ChangedLinkedOptions? changedLinkedQuestion = @event.ChangedLinkedQuestions.SingleOrDefault(x => x.QuestionId == this.Identity);

            if (changedLinkedQuestion != null)
            {
                await this.RefreshOptionsFromModelAsync();
            }
        }

        public async Task HandleAsync(RosterInstancesTitleChanged @event)
        {
            var optionListShouldBeUpdated = @event.ChangedInstances.Any(x => this.parentRosterIds.Contains(x.RosterInstance.GroupId));
            if (optionListShouldBeUpdated)
            {
                await this.RefreshOptionsFromModelAsync();
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

        private async Task RefreshOptionsFromModelAsync()
        {
            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
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

        private SingleOptionLinkedQuestionOptionViewModel CreateOptionViewModel(RosterVector linkedOption, RosterVector? answeredOption, IStatefulInterview interview)
        {
            var optionViewModel = new SingleOptionLinkedQuestionOptionViewModel
            {
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
