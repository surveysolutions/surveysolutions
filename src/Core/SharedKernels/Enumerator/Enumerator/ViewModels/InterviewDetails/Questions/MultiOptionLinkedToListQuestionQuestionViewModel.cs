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
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedToListQuestionQuestionViewModel : MvxNotifyPropertyChanged,
        IMultiOptionQuestionViewModelToggleable,
        IInterviewEntityViewModel,
        ILiteEventHandler<TextListQuestionAnswered>,
        ILiteEventHandler<MultipleOptionsQuestionAnswered>,
        ILiteEventHandler<AnswersRemoved>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPrincipal userIdentity;

        protected readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private readonly QuestionInstructionViewModel instructionViewModel;
        private readonly QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionState;

        protected IStatefulInterview interview;
        private Guid interviewId;
        private Identity questionIdentity;
        private Guid userId;
        private Guid linkedToQuestionId;
        private int? maxAllowedAnswers;
        private bool areAnswersOrdered;

        public QuestionInstructionViewModel InstructionViewModel => this.instructionViewModel;
        public IQuestionStateViewModel QuestionState => this.questionState;
        public AnsweringViewModel Answering { get; private set; }

        public CovariantObservableCollection<MultiOptionQuestionOptionViewModel> options { get; private set; }

        private OptionBorderViewModel<MultipleOptionsQuestionAnswered> optionsTopBorderViewModel;
        private OptionBorderViewModel<MultipleOptionsQuestionAnswered> optionsBottomBorderViewModel;

        public CovariantObservableCollection<MultiOptionQuestionOptionViewModel> Options
        {
            get { return this.options; }
            private set { this.options = value; this.RaisePropertyChanged(() => this.HasOptions); }
        }

        public bool HasOptions => this.Options.Any();

        public MultiOptionLinkedToListQuestionQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionState,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IPrincipal userIdentity, ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.Options = new CovariantObservableCollection<MultiOptionQuestionOptionViewModel>();
            this.questionState = questionState;
            this.questionnaireStorage = questionnaireStorage;
            this.eventRegistry = eventRegistry;
            this.userIdentity = userIdentity;
            this.instructionViewModel = instructionViewModel;
            this.interviewRepository = interviewRepository;
            this.Answering = answering;
            this.mainThreadDispatcher = mainThreadDispatcher;
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

            this.instructionViewModel.Init(interviewId, entityIdentity);
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


        protected void InitFromModel(IQuestionnaire questionnaire)
        {
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(questionIdentity.Id);
            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(questionIdentity.Id);
        }

        protected List<MultiOptionQuestionOptionViewModel> CreateOptions()
        {
            var linkedToListQuestion = interview.GetMultiOptionLinkedToListQuestion(this.questionIdentity);
            var answeredOptions = linkedToListQuestion.GetAnswer()?.ToDecimals()?.ToArray();

            var listQuestion = interview.FindTextListQuestionInQuestionBranch(this.linkedToQuestionId, this.questionIdentity);
            var listQuestionAnsweredOptions = listQuestion?.GetAnswer()?.Rows ?? new List<TextListAnswerRow>(); ;

            return listQuestionAnsweredOptions.Select(
                linkedOption => this.CreateOptionViewModel(linkedOption, answeredOptions)).ToList();
                
        }
        
        public void Handle(TextListQuestionAnswered @event)
        {
            if (@event.QuestionId == this.linkedToQuestionId)
            {
                //check scope and than update

                var newOptions = this.CreateOptions();

                this.mainThreadDispatcher.RequestMainThreadAction(() =>
                {
                    this.Options.SynchronizeWith(newOptions.ToList(), (s, t) => s.Value == t.Value);
                    this.RaisePropertyChanged(() => HasOptions);
                });
            }
        }

        private MultiOptionQuestionOptionViewModel CreateOptionViewModel(TextListAnswerRow optionValue, decimal[] answeredOptions)
        {
         var option =  new MultiOptionQuestionOptionViewModel(this)
            {
                Title = optionValue.Text,
                Value = Convert.ToInt32(optionValue.Value),
                Checked = answeredOptions?.Contains(optionValue.Value) ?? false,
                QuestionState = this.questionState
                
            };

            var indexOfAnswer = Array.IndexOf(answeredOptions ?? new decimal[] { }, optionValue.Value);
            option.CheckedOrder = this.areAnswersOrdered && indexOfAnswer >= 0 ? indexOfAnswer + 1 : (int?)null;

            return option;
        }


        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
        }

        public async Task ToggleAnswerAsync(MultiOptionQuestionOptionViewModelBase changedModel)
        {
            List<MultiOptionQuestionOptionViewModel> allSelectedOptions =
                this.areAnswersOrdered ?
                this.Options.Where(x => x.Checked).OrderBy(x => x.CheckedTimeStamp).ThenBy(x => x.CheckedOrder ?? 0).ToList() :
                this.Options.Where(x => x.Checked).ToList();

            if (this.maxAllowedAnswers.HasValue && allSelectedOptions.Count > this.maxAllowedAnswers)
            {
                changedModel.Checked = false;
                return;
            }

            
            var selectedValues = allSelectedOptions.Select(x => x.Value).ToArray();

            var command = new AnswerMultipleOptionsQuestionCommand(
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

        public void Dispose()
        {
            this.questionState.Dispose();

        }

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                this.optionsTopBorderViewModel = new OptionBorderViewModel<MultipleOptionsQuestionAnswered>(this.questionState, true)
                {
                    HasOptions = HasOptions
                };
                result.Add(this.optionsTopBorderViewModel);
                result.AddCollection(this.Options);
                this.optionsBottomBorderViewModel = new OptionBorderViewModel<MultipleOptionsQuestionAnswered>(this.questionState, false)
                {
                    HasOptions = HasOptions
                };
                result.Add(this.optionsBottomBorderViewModel);
                return result;
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    foreach (var option in this.Options.Where(option => option.Checked))
                    {
                        option.Checked = false;
                        option.CheckedOrder = null;
                    }
                }
            }
        }
    }
}