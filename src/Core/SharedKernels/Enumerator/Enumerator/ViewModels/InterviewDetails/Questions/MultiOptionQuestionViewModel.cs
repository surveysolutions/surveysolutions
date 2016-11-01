using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionQuestionViewModel : MvxNotifyPropertyChanged, 
        ICompositeQuestionWithChildren,
        IInterviewEntityViewModel,
        ILiteEventHandler<MultipleOptionsQuestionAnswered>,
        IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPrincipal principal;
        private readonly IUserInteractionService userInteraction;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private readonly QuestionInstructionViewModel instructionViewModel;
        private readonly QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionState;

        private Guid interviewId;
        private Identity questionIdentity;
        private Guid userId;
        private int? maxAllowedAnswers;
        private bool isRosterSizeQuestion;
        private bool areAnswersOrdered;

        public QuestionInstructionViewModel InstructionViewModel => this.instructionViewModel;
        public IQuestionStateViewModel QuestionState => this.questionState;
        public AnsweringViewModel Answering { get; private set; }

        public MultiOptionQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository,
            ILiteEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IPrincipal principal,
            IUserInteractionService userInteraction,
            AnsweringViewModel answering,
            FilteredOptionsViewModel filteredOptionsViewModel, 
            QuestionInstructionViewModel instructionViewModel)
        {
            this.Options = new CovariantObservableCollection<MultiOptionQuestionOptionViewModel>();
            this.questionState = questionStateViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.eventRegistry = eventRegistry;
            this.principal = principal;
            this.userInteraction = userInteraction;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.instructionViewModel = instructionViewModel;
            this.interviewRepository = interviewRepository;
            this.Answering = answering;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.eventRegistry.Subscribe(this, interviewId);
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.instructionViewModel.Init(interviewId, entityIdentity);
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity, 200);

            this.questionIdentity = entityIdentity;
            this.userId = this.principal.CurrentUserIdentity.UserId;
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(entityIdentity.Id);
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(entityIdentity.Id);
            this.isRosterSizeQuestion = questionnaire.ShouldQuestionSpecifyRosterSize(entityIdentity.Id);

            this.UpdateQuestionOptions();

            filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;
        }

        private void UpdateQuestionOptions()
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId.FormatGuid());
            MultiOptionAnswer existingAnswer = interview.GetMultiOptionAnswer(questionIdentity);
            var optionViewModels = this.filteredOptionsViewModel.GetOptions()
                .Select((x, index) => this.ToViewModel(x, existingAnswer))
                .ToList();

            this.Options.ForEach(x => x.DisposeIfDisposable());
            this.Options.Clear();

            optionViewModels.ForEach(x => this.Options.Add(x));
        }

        private void FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs)
        {
            this.UpdateQuestionOptions();
            this.RaisePropertyChanged(() => Options);
        }

        public void Dispose()
        {
            filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;

            this.filteredOptionsViewModel.Dispose();
            this.eventRegistry.Unsubscribe(this);
            this.questionState.Dispose();
        }

        public CovariantObservableCollection<MultiOptionQuestionOptionViewModel> Options { get; private set; }

        public bool HasOptions => true;

        private MultiOptionQuestionOptionViewModel ToViewModel(CategoricalOption model, MultiOptionAnswer multiOptionAnswer)
        {
            var result = new MultiOptionQuestionOptionViewModel(this)
            {
                Value = model.Value,
                Title = model.Title,
                Checked = multiOptionAnswer != null &&
                          multiOptionAnswer.IsAnswered &&
                          multiOptionAnswer.Answers.Any(x => model.Value == x),
            };
            var indexOfAnswer = Array.IndexOf(multiOptionAnswer.Answers ?? new decimal[]{}, model.Value);

            result.CheckedOrder = this.areAnswersOrdered && indexOfAnswer >= 0 ? indexOfAnswer + 1 : (int?) null;
            result.QuestionState = this.questionState;

            return result;
        }

        public async Task ToggleAnswerAsync(MultiOptionQuestionOptionViewModel changedModel)
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

            if (this.isRosterSizeQuestion && !changedModel.Checked)
            {
                var amountOfRostersToRemove = 1;
                var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage,
                    amountOfRostersToRemove);
                if (!(await this.userInteraction.ConfirmAsync(message)))
                {
                    changedModel.Checked = true;
                    return;
                }
            }

            var selectedValues = allSelectedOptions.Select(x => x.Value).ToArray();

            var command = new AnswerMultipleOptionsQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                selectedValues.Select(v => (int) v).ToArray());

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

        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
            if (this.areAnswersOrdered && @event.QuestionId == this.questionIdentity.Id && @event.RosterVector.Identical(this.questionIdentity.RosterVector))
            {
                this.PutOrderOnOptions(@event);
            }
        }

        private void PutOrderOnOptions(MultipleOptionsQuestionAnswered @event)
        {
            foreach (var option in this.Options)
            {
                var selectedOptionIndex = Array.IndexOf(@event.SelectedValues, option.Value);

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

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                result.Add(new OptionBorderViewModel<MultipleOptionsQuestionAnswered>(this.questionState, true));
                result.AddCollection(this.Options);
                result.Add(new OptionBorderViewModel<MultipleOptionsQuestionAnswered>(this.questionState, false));
                return result;
            }
        }
    }
}