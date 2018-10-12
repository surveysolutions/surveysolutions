using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class YesNoQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IDisposable,
        ICompositeQuestionWithChildren,
        ILiteEventHandler<YesNoQuestionAnswered>,
        ILiteEventHandler<AnswersRemoved>
    {
        private readonly Guid userId;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        private readonly IUserInteractionService userInteraction;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private Guid interviewId;
        private string interviewIdAsString;
        public bool AreAnswersOrdered { get; protected set; }
        private int? maxAllowedAnswers;
        private bool isRosterSizeQuestion;
        private readonly QuestionStateViewModel<YesNoQuestionAnswered> questionState;
        private string maxAnswersCountMessage;

        public AnsweringViewModel Answering { get; set; }

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public CovariantObservableCollection<YesNoQuestionOptionViewModel> Options { get; set; }

        public string MaxAnswersCountMessage
        {
            get => maxAnswersCountMessage;
            set => SetProperty(ref maxAnswersCountMessage, value);
        }

        public YesNoQuestionViewModel(IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher,
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
            this.mainThreadDispatcher = mainThreadDispatcher;

            this.questionState = questionStateViewModel;
            this.Answering = answering;
            this.userInteraction = userInteraction;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Options = new CovariantObservableCollection<YesNoQuestionOptionViewModel>();
            this.timer = new Timer(async _ => { await SaveAnswer(); }, null, Timeout.Infinite, Timeout.Infinite);
        }

        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            interviewIdAsString = interviewId;

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity, 200);

            this.InstructionViewModel.Init(interviewId, entityIdentity);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire =
                this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.AreAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(entityIdentity.Id);
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(entityIdentity.Id);
            this.isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(entityIdentity.Id);
            this.Identity = entityIdentity;
            this.interviewId = interview.Id;

            this.UpdateQuestionOptions();

            PreviousOptionToReset = interview.GetYesNoQuestion(this.Identity)
                .GetAnswer()
                ?.CheckedOptions
                .Select(x => new AnsweredYesNoOption(x.Value, x.Yes)).ToList();

            this.filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void UpdateQuestionOptions()
        {
            var interview = this.interviewRepository.Get(interviewIdAsString);
            var answerModel = interview.GetYesNoQuestion(this.Identity);

            var checkedYesNoAnswerOptions = answerModel.GetAnswer()?.ToAnsweredYesNoOptions()?.ToArray() ??
                                            Array.Empty<AnsweredYesNoOption>();

            var newOptions = this.filteredOptionsViewModel.GetOptions()
                .Select(model => this.ToViewModel(model, checkedYesNoAnswerOptions, answerModel))
                .ToList();

            this.Options.ForEach(x => x.DisposeIfDisposable());

            this.Options.Clear();
            newOptions.ForEach(x => this.Options.Add(x));
            UpateMaxAnswersCountMessage(checkedYesNoAnswerOptions.Count(x => x.Yes));
        }

        private async void FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs)
        {
            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.UpdateQuestionOptions();
                this.RaisePropertyChanged(() => Options);
            });
        }

        private YesNoQuestionOptionViewModel ToViewModel(CategoricalOption model,
            AnsweredYesNoOption[] checkedYesNoAnswerOptions,
            InterviewTreeYesNoQuestion treeQuestion)
        {
            var isExistAnswer = checkedYesNoAnswerOptions != null &&
                                checkedYesNoAnswerOptions.Any(a => a.OptionValue == model.Value);
            bool? isSelected = isExistAnswer
                ? checkedYesNoAnswerOptions.First(a => a.OptionValue == model.Value).Yes
                : (bool?) null;
            var yesAnswerCheckedOrder = isExistAnswer && this.AreAnswersOrdered
                ? Array.IndexOf(checkedYesNoAnswerOptions.Where(am => am.Yes).Select(am => am.OptionValue).ToArray(),
                      model.Value) + 1
                : (int?) null;
            var answerCheckedOrder = isExistAnswer && this.AreAnswersOrdered
                ? Array.IndexOf(checkedYesNoAnswerOptions.Select(am => am.OptionValue).ToArray(), model.Value) + 1
                : (int?) null;

            var optionViewModel = new YesNoQuestionOptionViewModel(this, this.questionState)
            {
                Value = model.Value,
                Title = model.Title,
                Selected = isSelected,
                YesAnswerCheckedOrder = yesAnswerCheckedOrder,
                AnswerCheckedOrder = answerCheckedOrder,
                IsProtected = treeQuestion.IsAnswerProtected(model.Value),
                YesCanBeChecked = isSelected.GetValueOrDefault() || !maxAllowedAnswers.HasValue ||
                                  checkedYesNoAnswerOptions.Count(x => x.Yes) < maxAllowedAnswers
            };

            return optionViewModel;
        }

        private readonly Timer timer;
        protected internal int ThrottlePeriod { get; set; } = Constants.ThrottlePeriod;

        private List<AnsweredYesNoOption> previousOptionToReset = null;
        private List<AnsweredYesNoOption> PreviousOptionToReset
        {
            get => previousOptionToReset ?? new List<AnsweredYesNoOption>();
            set => previousOptionToReset = value;
        }

        private List<AnsweredYesNoOption> selectedOptionsToSave = null;

        private async Task SaveAnswer()
        {
            if (this.userInteraction.HasPendingUserInterations)
            {
                await this.userInteraction.WaitPendingUserInteractionsAsync();
                ResetUiOptions();
                return;
            }

            if (this.isRosterSizeQuestion)
            {
                var itemsToDelete = PreviousOptionToReset.Where(x => x.Yes)
                    .Except(selectedOptionsToSave.Where(x => x.Yes)).ToList();

                if (itemsToDelete.Any())
                {
                    var amountOfRostersToRemove = itemsToDelete.Count;
                    var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                    if (!await this.userInteraction.ConfirmAsync(message))
                    {
                        ResetUiOptions();
                        return;
                    }
                }
            }

            var command = new AnswerYesNoQuestion(
                this.interviewId,
                this.userId,
                this.Identity.Id,
                this.Identity.RosterVector,
                selectedOptionsToSave.ToArray());

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
                PreviousOptionToReset = selectedOptionsToSave.ToList();
            }
            catch (InterviewException ex)
            {
                if (ex.ExceptionType != InterviewDomainExceptionType.QuestionIsMissing)
                {
                    // reset to previous state
                    ResetUiOptions();
                }
                
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void ResetUiOptions()
        {
            var interview = this.interviewRepository.Get(this.interviewId.FormatGuid());
            var answer = interview.GetYesNoQuestion(this.Identity)?.GetAnswer()?.CheckedOptions;
            if (answer == null) return;

            PreviousOptionToReset = answer.Select(x => new AnsweredYesNoOption(x.Value, x.Yes)).ToList();
            PutOrderOnOptions(PreviousOptionToReset.ToArray());
        }

        public async Task ToggleAnswerAsync(YesNoQuestionOptionViewModel changedModel, bool? oldValue)
        {
            List<YesNoQuestionOptionViewModel> allSelectedOptions =
                this.AreAnswersOrdered
                    ? this.Options.Where(x => x.Selected.HasValue).OrderBy(x => x.AnswerCheckedOrder).ToList()
                    : this.Options.Where(x => x.Selected.HasValue).ToList();

            int countYesSelectedOptions = allSelectedOptions.Count(o => o.YesSelected);

            if (this.maxAllowedAnswers.HasValue && countYesSelectedOptions > this.maxAllowedAnswers)
            {
                changedModel.Selected = oldValue;
                return;
            }

            var selectedValuesWithoutJustChanged = allSelectedOptions.Except(x => x.Value == changedModel.Value)
                .Select(x => new AnsweredYesNoOption(x.Value, x.YesSelected));

            this.selectedOptionsToSave = changedModel.Selected.HasValue
                ? selectedValuesWithoutJustChanged
                    .Union(new AnsweredYesNoOption(changedModel.Value, changedModel.Selected.Value).ToEnumerable())
                    .ToList()
                : selectedValuesWithoutJustChanged.ToList();

            if (this.ThrottlePeriod == 0)
            {
                await SaveAnswer();
            }
            else
            {
                timer.Change(ThrottlePeriod, Timeout.Infinite);
            }
        }

        public void Dispose()
        {
            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();

            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
        }

        public void Handle(AnswersRemoved @event)
        {
            if (@event.Questions.Any(x =>
                x.Id == this.Identity.Id && x.RosterVector.Identical(this.Identity.RosterVector)))
            {
                if (this.AreAnswersOrdered || this.maxAllowedAnswers.HasValue)
                {
                    foreach (var option in this.Options.ToList())
                    {
                        option.Selected = null;
                        option.YesAnswerCheckedOrder = null;
                        option.YesCanBeChecked = true;
                    }
                }

                PreviousOptionToReset = null;

                UpateMaxAnswersCountMessage(0);
            }
        }

        public void Handle(YesNoQuestionAnswered @event)
        {
            if (@event.QuestionId == this.Identity.Id && @event.RosterVector.Identical(this.Identity.RosterVector))
            {
                if (this.AreAnswersOrdered || this.maxAllowedAnswers.HasValue)
                {
                    this.PutOrderOnOptions(@event.AnsweredOptions);
                }

                UpateMaxAnswersCountMessage(@event.AnsweredOptions.Count(x => x.Yes));
            }
        }

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                result.Add(new OptionBorderViewModel(this.questionState, true));
                result.AddCollection(this.Options);
                result.Add(new OptionBorderViewModel(this.questionState, false));
                return result;
            }
        }

        private void PutOrderOnOptions(AnsweredYesNoOption[] answeredYesNoOptions)
        {
            var orderedOptions = answeredYesNoOptions.Select(ao => ao.OptionValue).ToList();
            var orderedYesOptions = answeredYesNoOptions.Where(ao => ao.Yes).Select(ao => ao.OptionValue).ToList();
            var maxAnswersCountNotReachedYet =
                !this.maxAllowedAnswers.HasValue || orderedYesOptions.Count < this.maxAllowedAnswers;

            foreach (var option in this.Options.ToList())
            {
                var selectedOptionIndex = orderedOptions.IndexOf(option.Value);

                if (selectedOptionIndex >= 0)
                {
                    var answeredYesNoOption = answeredYesNoOptions[selectedOptionIndex];
                    option.Selected = answeredYesNoOption.Yes;
                    option.YesAnswerCheckedOrder = answeredYesNoOption.Yes && this.AreAnswersOrdered
                        ? orderedYesOptions.IndexOf(option.Value) + 1
                        : (int?) null;
                    option.AnswerCheckedOrder = orderedOptions.IndexOf(option.Value) + 1;
                    option.YesCanBeChecked = answeredYesNoOption.Yes || maxAnswersCountNotReachedYet;
                }
                else
                {
                    option.YesAnswerCheckedOrder = null;
                    option.AnswerCheckedOrder = null;
                    option.Selected = null;
                    option.YesCanBeChecked = maxAnswersCountNotReachedYet;
                }
            }
        }

        private void UpateMaxAnswersCountMessage(int answersCount)
        {
            if (this.maxAllowedAnswers.HasValue)
            {
                this.MaxAnswersCountMessage = string.Format(UIResources.Interview_MaxAnswersCount,
                    answersCount, Math.Min(this.maxAllowedAnswers.Value, this.Options.Count));
            }
        }
    }
}
