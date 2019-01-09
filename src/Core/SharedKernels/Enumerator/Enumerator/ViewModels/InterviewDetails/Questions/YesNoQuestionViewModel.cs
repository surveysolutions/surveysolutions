using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private readonly ThrottlingViewModel throttlingModel;
        private Guid interviewId;
        private string interviewIdAsString;
        private bool areAnswersOrdered { get; set; }
        private int? maxAllowedAnswers;
        private bool isRosterSizeQuestion;
        private readonly QuestionStateViewModel<YesNoQuestionAnswered> questionState;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private string maxAnswersCountMessage;

        public AnsweringViewModel Answering { get; set; }

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public CovariantObservableCollection<YesNoQuestionOptionViewModel> Options { get; } =
            new CovariantObservableCollection<YesNoQuestionOptionViewModel>();

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
            IInterviewViewModelFactory viewModelFactory,
            FilteredOptionsViewModel filteredOptionsViewModel,
            QuestionInstructionViewModel instructionViewModel, 
            ThrottlingViewModel throttlingModel)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            this.userId = principal.CurrentUserIdentity.UserId;

            this.questionnaireRepository = questionnaireRepository ?? throw new ArgumentNullException(nameof(questionnaireRepository));
            this.interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher;

            this.questionState = questionStateViewModel;
            this.viewModelFactory = viewModelFactory;
            this.Answering = answering;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.throttlingModel = throttlingModel;
        }

        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewIdAsString = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity, 200);
            this.throttlingModel.Init(SaveAnswer);

            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(entityIdentity.Id);
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(entityIdentity.Id);
            this.isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(entityIdentity.Id);
            this.Identity = entityIdentity;
            this.interviewId = interview.Id;

            this.UpdateViewModels();

            this.filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void UpdateViewModels()
        {
            var interview = this.interviewRepository.Get(interviewIdAsString);
            var interviewQuestion = interview.GetYesNoQuestion(this.Identity);

            var newOptions = this.filteredOptionsViewModel.GetOptions()
                .Select(option => this.ToViewModel(option, interviewQuestion))
                .ToList();

            this.Options.ForEach(x => x.DisposeIfDisposable());
            this.Options.ReplaceWith(newOptions);

            this.UpdateOptionsFromInterview();
        }

        private async void FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs)
        {
            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.UpdateViewModels();
                this.RaisePropertyChanged(() => Options);
            });
        }

        private YesNoQuestionOptionViewModel ToViewModel(CategoricalOption option, InterviewTreeYesNoQuestion interviewQuestion)
        {
            var optionViewModel = this.viewModelFactory.GetNew<YesNoQuestionOptionViewModel>();

            optionViewModel.Init(this.Identity, option, this.questionState, this.isRosterSizeQuestion,
                interviewQuestion.IsAnswerProtected(option.Value), async () => await this.UpdateOptionsToSaveAsync(optionViewModel));

            return optionViewModel;
        }

        private List<AnsweredYesNoOption> selectedOptionsToSave;

        private async Task SaveAnswer()
        {
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
            }
            catch (InterviewException ex)
            {
                if (ex.ExceptionType != InterviewDomainExceptionType.QuestionIsMissing)
                {
                    // reset to previous state
                    this.UpdateOptionsFromInterview();
                }
                
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void UpdateOptionsFromInterview()
        {
            var interview = this.interviewRepository.Get(this.interviewIdAsString);

            var answeredOptions =
                interview.GetYesNoQuestion(this.Identity).GetAnswer()?.ToAnsweredYesNoOptions()?.ToArray() ??
                Array.Empty<AnsweredYesNoOption>();

            this.UpdateViewModelsByAnsweredOptions(answeredOptions);
        }

        public async Task UpdateOptionsToSaveAsync(YesNoQuestionOptionViewModel optionViewModel)
        {
            var allSelectedOptions = this.Options.Where(x => x.YesSelected || x.NoSelected).ToArray();

            if (this.areAnswersOrdered && optionViewModel.YesSelected)
                optionViewModel.YesAnswerCheckedOrder = allSelectedOptions.Count(x => x.YesSelected);

            this.selectedOptionsToSave = allSelectedOptions.OrderBy(x => x.YesAnswerCheckedOrder)
                .Select(x => new AnsweredYesNoOption(x.Value, x.YesSelected)).ToList();

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        public void Dispose()
        {
            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();

            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
            this.throttlingModel.Dispose();
        }

        public void Handle(AnswersRemoved @event)
        {
            if (!@event.Questions.Any(x => x.Id == this.Identity.Id && x.RosterVector.Identical(this.Identity.RosterVector))) return;

            this.UpdateOptionsFromInterview();
        }

        public void Handle(YesNoQuestionAnswered @event)
        {
            if (@event.QuestionId != this.Identity.Id || !@event.RosterVector.Identical(this.Identity.RosterVector)) return;

            this.UpdateViewModelsByAnsweredOptions(@event.AnsweredOptions);
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

        private void UpdateViewModelsByAnsweredOptions(AnsweredYesNoOption[] answeredYesNoOptions)
        {
            var orderedYesOptions = answeredYesNoOptions.Where(ao => ao.Yes).Select(ao => ao.OptionValue).ToList();

            foreach (var option in this.Options)
            {
                var answeredOption = answeredYesNoOptions.FirstOrDefault(x => x.OptionValue == option.Value);

                option.YesSelected = answeredOption?.Yes == true;
                option.NoSelected = answeredOption?.Yes == false;

                if (this.areAnswersOrdered)
                    option.YesAnswerCheckedOrder = option.YesSelected ? orderedYesOptions.IndexOf(option.Value) + 1 : (int?) null;

                if (this.maxAllowedAnswers.HasValue)
                    option.YesCanBeChecked = option.YesSelected || orderedYesOptions.Count < this.maxAllowedAnswers;
            }

            if (this.maxAllowedAnswers.HasValue)
            {
                this.MaxAnswersCountMessage = string.Format(UIResources.Interview_MaxAnswersCount,
                    orderedYesOptions.Count, Math.Min(this.maxAllowedAnswers.Value, this.Options.Count));
            }
        }
    }
}
