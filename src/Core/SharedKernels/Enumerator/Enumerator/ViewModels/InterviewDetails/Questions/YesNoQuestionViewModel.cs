using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
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
    public class YesNoQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IDisposable,
        ICompositeQuestionWithChildren,
        ILiteEventHandler<YesNoQuestionAnswered>
    {
        private readonly Guid userId;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IUserInteractionService userInteraction;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private Guid interviewId;
        private string interviewIdAsString;
        private bool areAnswersOrdered;
        private int? maxAllowedAnswers;
        private bool isRosterSizeQuestion;
        private readonly QuestionStateViewModel<YesNoQuestionAnswered> questionState;

        public AnsweringViewModel Answering { get; set; }

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public List<YesNoQuestionOptionViewModel> Options { get; set; }

        public YesNoQuestionViewModel(IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            QuestionStateViewModel<YesNoQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            IUserInteractionService userInteraction,
            FilteredOptionsViewModel filteredOptionsViewModel,
            QuestionInstructionViewModel instructionViewModel)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));
            if (interviewRepository == null) throw new ArgumentNullException(nameof(interviewRepository));

            this.userId = principal.CurrentUserIdentity.UserId;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.questionState = questionStateViewModel;
            this.Answering = answering;
            this.userInteraction = userInteraction;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.InstructionViewModel = instructionViewModel;
        }

        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            interviewIdAsString = interviewId;

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity);

            this.InstructionViewModel.Init(interviewId, entityIdentity);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(entityIdentity.Id);
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(entityIdentity.Id);
            this.isRosterSizeQuestion = questionnaire.ShouldQuestionSpecifyRosterSize(entityIdentity.Id);
            this.Identity = entityIdentity;
            this.interviewId = interview.Id;

            this.UpdateQuestionOptions();

            this.filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void UpdateQuestionOptions()
        {
            var interview = this.interviewRepository.Get(interviewIdAsString);
            var answerModel = interview.GetYesNoAnswer(Identity);

            this.Options = this.filteredOptionsViewModel.GetOptions()
                .Select(model => this.ToViewModel(model, answerModel))
                .ToList();
        }

        private void FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs)
        {
            this.UpdateQuestionOptions();
            this.RaisePropertyChanged(() => Options);
        }

        private YesNoQuestionOptionViewModel ToViewModel(CategoricalOption model, YesNoAnswer answerModel)
        {
            var isExistAnswer = answerModel.Answers != null && answerModel.Answers.Any(a => a.OptionValue == model.Value);
            var isSelected = isExistAnswer 
                ? answerModel.Answers.First(a => a.OptionValue == model.Value).Yes 
                : (bool?) null;
            var yesAnswerCheckedOrder = isExistAnswer && this.areAnswersOrdered
                ? Array.IndexOf(answerModel.Answers.Where(am => am.Yes).Select(am => am.OptionValue).ToArray(), model.Value) + 1
                : (int?)null;
            var answerCheckedOrder = isExistAnswer && this.areAnswersOrdered
                ? Array.IndexOf(answerModel.Answers.Select(am => am.OptionValue).ToArray(), model.Value) + 1
                : (int?)null;

            var optionViewModel = new YesNoQuestionOptionViewModel(this, this.questionState)
            {
                Value = model.Value,
                Title = model.Title,
                Selected = isSelected,
                YesAnswerCheckedOrder = yesAnswerCheckedOrder,
                AnswerCheckedOrder = answerCheckedOrder
            };

            return optionViewModel;
        }

        public async Task ToggleAnswerAsync(YesNoQuestionOptionViewModel changedModel)
        {
            List<YesNoQuestionOptionViewModel> allSelectedOptions =
                this.areAnswersOrdered
                ? this.Options.Where(x => x.Selected.HasValue).OrderBy(x => x.AnswerCheckedOrder).ToList()
                : this.Options.Where(x => x.Selected.HasValue).ToList();

            var interview = this.interviewRepository.Get(interviewIdAsString);

            int countYesSelectedOptions = allSelectedOptions.Count(o => o.YesSelected);

            if (this.maxAllowedAnswers.HasValue && countYesSelectedOptions > this.maxAllowedAnswers)
            {
                var answerModel = interview.GetYesNoAnswer(Identity);
                var answeredYesNoOption = answerModel.Answers.FirstOrDefault(yn => yn.OptionValue == changedModel.Value);
                changedModel.Selected = answeredYesNoOption?.Yes;
                return;
            }

            if (this.isRosterSizeQuestion && (!changedModel.Selected.HasValue || !changedModel.Selected.Value))
            {
                var answerModel = interview.GetYesNoAnswer(Identity);

                var backendYesAnswersCount = answerModel?.Answers?.Count(a => a.Yes) ?? 0;
                var UIYesAnswersCount = this.Options.Count(o => o.YesSelected);

                if (backendYesAnswersCount > UIYesAnswersCount)
                {
                    var amountOfRostersToRemove = 1;
                    var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                    if (!(await this.userInteraction.ConfirmAsync(message)))
                    {
                        changedModel.Selected = true;
                        return;
                    }
                }
            }

            var selectedValuesWithoutJustChanged = allSelectedOptions.Except(x => x.Value == changedModel.Value).Select(x => new AnsweredYesNoOption(x.Value, x.YesSelected));

            var selectedValuesWithJustChanged
                = changedModel.Selected.HasValue
                    ? selectedValuesWithoutJustChanged.Union(new AnsweredYesNoOption(changedModel.Value, changedModel.Selected.Value).ToEnumerable()).ToArray()
                    : selectedValuesWithoutJustChanged.ToArray();

            var command = new AnswerYesNoQuestion(
                this.interviewId,
                this.userId,
                this.Identity.Id,
                this.Identity.RosterVector,
                DateTime.UtcNow,
                selectedValuesWithJustChanged);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command).ConfigureAwait(false);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        // TODO: delete this method if KP-6577 is verified
        public async Task ToggleAnswerAsyncOld(YesNoQuestionOptionViewModel changedModel)
        {
            List<YesNoQuestionOptionViewModel> allSelectedOptions =
                this.areAnswersOrdered
                ? this.Options.Where(x => x.Selected.HasValue).OrderBy(x => x.AnswerCheckedOrder).ToList() 
                : this.Options.Where(x => x.Selected.HasValue).ToList();

            var interview = this.interviewRepository.Get(interviewIdAsString);

            int countYesSelectedOptions = allSelectedOptions.Count(o => o.YesSelected);

            if (this.maxAllowedAnswers.HasValue && countYesSelectedOptions > this.maxAllowedAnswers)
            {
                var answerModel = interview.GetYesNoAnswer(Identity);
                var answeredYesNoOption = answerModel.Answers.FirstOrDefault(yn => yn.OptionValue == changedModel.Value);
                changedModel.Selected = answeredYesNoOption?.Yes;
                return;
            }

            if (this.isRosterSizeQuestion && (!changedModel.Selected.HasValue || !changedModel.Selected.Value))
            {
                var answerModel = interview.GetYesNoAnswer(Identity);

                var backendYesAnswersCount = answerModel?.Answers?.Count(a => a.Yes) ?? 0;
                var UIYesAnswersCount = this.Options.Count(o => o.YesSelected);

                if (backendYesAnswersCount > UIYesAnswersCount)
                {
                    var amountOfRostersToRemove = 1;
                    var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                    if (!(await this.userInteraction.ConfirmAsync(message)))
                    {
                        changedModel.Selected = true;
                        return;
                    }
                }
            }

            YesNoAnswer actualAnswerModel = interview.GetYesNoAnswer(this.Identity);

            var selectedValuesWithoutJustChanged = actualAnswerModel.Answers?.Except(x => x.OptionValue == changedModel.Value) ?? Enumerable.Empty<AnsweredYesNoOption>();

            var selectedValuesWithJustChanged
                = changedModel.Selected.HasValue
                    ? selectedValuesWithoutJustChanged.Union(new AnsweredYesNoOption(changedModel.Value, changedModel.Selected.Value).ToEnumerable()).ToArray()
                    : selectedValuesWithoutJustChanged.ToArray();

            var command = new AnswerYesNoQuestion(
                this.interviewId,
                this.userId,
                this.Identity.Id,
                this.Identity.RosterVector,
                DateTime.UtcNow,
                selectedValuesWithJustChanged);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command).ConfigureAwait(false);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Dispose()
        {
            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();

            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
        }

        public void Handle(YesNoQuestionAnswered @event)
        {
            if (this.areAnswersOrdered && @event.QuestionId == this.Identity.Id && @event.RosterVector.Identical(this.Identity.RosterVector))
            {
                this.PutOrderOnOptions(@event);
            }
        }

        public CompositeCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                result.Add(new OptionBorderViewModel<YesNoQuestionAnswered>(this.questionState, true));
                result.AddCollection(new ObservableCollection<ICompositeEntity>(this.Options));
                result.Add(new OptionBorderViewModel<YesNoQuestionAnswered>(this.questionState, false));
                return result;
            }
        }

        private void PutOrderOnOptions(YesNoQuestionAnswered @event)
        {
            var orderedOptions = @event.AnsweredOptions.Select(ao => ao.OptionValue).ToList();
            var orderedYesOptions = @event.AnsweredOptions.Where(ao => ao.Yes).Select(ao => ao.OptionValue).ToList();

            foreach (var option in this.Options)
            {
                var selectedOptionIndex = orderedOptions.IndexOf(option.Value);

                if (selectedOptionIndex >= 0)
                {
                    var answeredYesNoOption = @event.AnsweredOptions[selectedOptionIndex];
                    option.YesAnswerCheckedOrder = answeredYesNoOption.Yes 
                        ? orderedYesOptions.IndexOf(option.Value) + 1
                        : (int?)null;
                    option.AnswerCheckedOrder = orderedOptions.IndexOf(option.Value) + 1;
                    option.Selected = answeredYesNoOption.Yes;
                }
                else
                {
                    option.YesAnswerCheckedOrder = null;
                    option.AnswerCheckedOrder = null;
                    option.Selected = null;
                }
            }
        }
    }
}