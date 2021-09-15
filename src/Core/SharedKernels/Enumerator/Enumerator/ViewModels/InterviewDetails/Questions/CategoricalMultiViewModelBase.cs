using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public abstract class CategoricalMultiViewModelBase<TOptionValue, TInterviewAnswer> : MvxNotifyPropertyChanged, 
        ICompositeQuestionWithChildren,
        IInterviewEntityViewModel, 
        IAsyncViewModelEventHandler<AnswersRemoved>,
        IDisposable
    {
        protected readonly ThrottlingViewModel throttlingModel;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IPrincipal principal;
        protected readonly IStatefulInterviewRepository interviewRepository;
        private Guid interviewId;
        private bool areAnswersOrdered;
        protected int? maxAllowedAnswers;

        public QuestionInstructionViewModel InstructionViewModel { get; }
        public IQuestionStateViewModel QuestionState { get; }
        public AnsweringViewModel Answering { get; }
        public virtual IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                result.Add(this.topBorderViewModel);
                result.AddCollection(this.Options);

                this.AddCustomViewModels(result);

                result.Add(this.bottomBorderViewModel);

                result.Add(this.bottomInfoViewModel);

                return result;
            }
        }

        private readonly OptionBorderViewModel topBorderViewModel;
        private readonly OptionBorderViewModel bottomBorderViewModel;
        private readonly CategoricalMultiBottomInfoViewModel bottomInfoViewModel;
        public CovariantObservableCollection<CategoricalMultiOptionViewModel<TOptionValue>> Options { get; set; }

        public Identity Identity { get; private set; }

        private bool hasOptions;
        public bool HasOptions
        {
            get => hasOptions;
            set => SetProperty(ref hasOptions, value);
        }

        protected CategoricalMultiViewModelBase(
            IQuestionStateViewModel questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository,
            IViewModelEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IPrincipal principal,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            ThrottlingViewModel throttlingModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.eventRegistry = eventRegistry;
            this.principal = principal;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;

            this.throttlingModel = throttlingModel;

            this.bottomInfoViewModel = new CategoricalMultiBottomInfoViewModel();
            this.topBorderViewModel = new OptionBorderViewModel(this.QuestionState, true);
            this.bottomBorderViewModel = new OptionBorderViewModel(this.QuestionState, false);
        }
        
        protected abstract void SaveAnsweredOptionsForThrottling(IOrderedEnumerable<CategoricalMultiOptionViewModel<TOptionValue>> answeredViewModels);
        protected abstract TInterviewAnswer[] GetAnsweredOptionsFromInterview(IStatefulInterview interview);
        protected abstract void SetAnswerToOptionViewModel(CategoricalMultiOptionViewModel<TOptionValue> optionViewModel, TInterviewAnswer answer);
        protected abstract void RemoveAnswerFromOptionViewModel(CategoricalMultiOptionViewModel<TOptionValue> optionViewModel);
        protected abstract AnswerQuestionCommand GetAnswerCommand(Guid interviewId, Guid userId);
        protected abstract IEnumerable<CategoricalMultiOptionViewModel<TOptionValue>> GetOptions(IStatefulInterview interview);
        protected abstract bool IsInterviewAnswer(TInterviewAnswer interviewAnswer, TOptionValue optionValue);
        protected virtual void Init(IStatefulInterview interview, IQuestionnaire questionnaire) { }
        protected virtual TInterviewAnswer[] FilterAnsweredOptions(TInterviewAnswer[] answeredOptions) => answeredOptions;
        protected virtual void AddCustomViewModels(CompositeCollection<ICompositeEntity> compositeCollection) { }

        public virtual void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);
            
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.Identity = entityIdentity;
            this.interviewId = interview.Id;
            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(entityIdentity.Id);
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(entityIdentity.Id);

            this.Init(interview, questionnaire);

            this.throttlingModel.Init(SaveAnswer);
            
            this.UpdateViewModelsAsync().WaitAndUnwrapException();

            this.eventRegistry.Subscribe(this, interviewId);
            this.bottomInfoViewModel.Identity = entityIdentity;
        }

        private async Task SaveAnswer()
        {
            try
            {
                var command = this.GetAnswerCommand(this.interviewId, this.principal.CurrentUserIdentity.UserId);
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                if (ex.ExceptionType != InterviewDomainExceptionType.QuestionIsMissing)
                {
                    // reset to previous state
                    await this.UpdateOptionsFromInterviewAsync(true);
                }

                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        protected virtual async Task UpdateViewModelsAsync()
        {
            var interview = this.interviewRepository.Get(this.interviewId.FormatGuid());

            await this.InvokeOnMainThreadAsync(
                () => this.Options.ReplaceWith(this.GetOptions(interview).ToList()));

            await this.UpdateOptionsFromInterviewAsync();
        }

        protected void InitViewModel(string title, TOptionValue value, IStatefulInterview interview,
            CategoricalMultiOptionViewModel<TOptionValue> vm, bool isAnswerProtected = false)
            => vm.Init(this.QuestionState, title, value, isAnswerProtected, async () => await this.ToggleAnswerAsync(vm));

        protected async Task ToggleAnswerAsync(CategoricalMultiOptionViewModel<TOptionValue> optionViewModel)
        {
            var allSelectedOptions = this.Options.Where(x => x.IsAnswered()).ToArray();

            if (this.areAnswersOrdered)
            {
                var order = optionViewModel.CheckedOrder;
                optionViewModel.CheckedOrder = optionViewModel.IsOrdered()
                    ? (int?) allSelectedOptions.Count(x => x.IsOrdered())
                    : null;

                if (optionViewModel.CheckedOrder == null && order.HasValue)
                {
                    foreach (var option in this.Options)
                    {
                        if (option.CheckedOrder > order)
                            option.CheckedOrder--;
                    }
                }
            }

            this.SaveAnsweredOptionsForThrottling(allSelectedOptions.OrderBy(x => x.CheckedOrder));

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        private async Task UpdateOptionsFromInterviewAsync(bool force = false)
        {
            if (!force && throttlingModel.HasPendingAction)
                return;
            
            var interview = this.interviewRepository.Get(this.interviewId.FormatGuid());
            var answeredOptions = this.GetAnsweredOptionsFromInterview(interview);

            await this.UpdateViewModelsByAnsweredOptionsAsync(answeredOptions, force);
        }

        protected async Task UpdateViewModelsByAnsweredOptionsAsync(TInterviewAnswer[] answeredOptions, bool force = false)
        {
            if (!force && throttlingModel.HasPendingAction)
                return;

            Trace.WriteLine("UpdateOptions: " + DateTime.Now.ToString("HH:mm:ss.fff"));
            answeredOptions ??= Array.Empty<TInterviewAnswer>();

            var filteredAnswers = this.FilterAnsweredOptions(answeredOptions);

            await this.InvokeOnMainThreadAsync(() =>
            {
                foreach (var option in this.Options)
                {
                    if (TryGetAnswer(answeredOptions, option.Value, out TInterviewAnswer answeredOption))
                        this.SetAnswerToOptionViewModel(option, answeredOption);
                    else
                        this.RemoveAnswerFromOptionViewModel(option);
                    
                    if (this.areAnswersOrdered)
                        option.CheckedOrder = option.Checked && answeredOption != null
                            ? Array.IndexOf(filteredAnswers, answeredOption) + 1
                            : (int?) null;

                    if (this.maxAllowedAnswers.HasValue)
                        option.CanBeChecked = option.Checked || filteredAnswers.Length < this.maxAllowedAnswers;
                }

                if (this.maxAllowedAnswers.HasValue)
                {
                    this.bottomInfoViewModel.MaxAnswersCountMessage = filteredAnswers.Length < this.maxAllowedAnswers
                        ? string.Empty 
                        : string.Format(UIResources.Interview_MaxAnswersCountSelected, this.maxAllowedAnswers.Value);
                }

                this.HasOptions = this.Options.Any();
                this.UpdateBorders();
            });
        }

        
        private bool TryGetAnswer(TInterviewAnswer[] answeredOptions, TOptionValue optionValue, out TInterviewAnswer interviewAnswer)
        {
            foreach (var option in answeredOptions) 
            {
                if (IsInterviewAnswer(option, optionValue))
                {
                    interviewAnswer = option;
                    return true;
                }
            }

            interviewAnswer = default;
            return false;
        }

        protected virtual void UpdateBorders()
        {
            this.topBorderViewModel.HasOptions = this.HasOptions;
            this.bottomBorderViewModel.HasOptions = this.HasOptions;
        }

        public virtual async Task HandleAsync(AnswersRemoved @event)
        {
            if (!@event.Questions.Any(x => x.Id == this.Identity.Id && x.RosterVector.Identical(this.Identity.RosterVector))
                || throttlingModel.HasPendingAction)
                return;

            await this.UpdateOptionsFromInterviewAsync();
        }

        public virtual void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);

            this.throttlingModel.Dispose();
            this.QuestionState.Dispose();
            this.InstructionViewModel.Dispose();
        }
    }
}
