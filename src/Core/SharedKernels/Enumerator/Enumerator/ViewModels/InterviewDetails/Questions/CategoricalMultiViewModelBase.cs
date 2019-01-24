using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
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
        ILiteEventHandler<AnswersRemoved>,
        IDisposable
    {
        private readonly ThrottlingViewModel throttlingModel;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private Guid interviewId;
        private bool areAnswersOrdered;
        private int? maxAllowedAnswers;

        public QuestionInstructionViewModel InstructionViewModel { get; }
        public IQuestionStateViewModel QuestionState { get; }
        public AnsweringViewModel Answering { get; }
        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                result.Add(new OptionBorderViewModel(this.QuestionState, true));
                result.AddCollection(this.Options);
                result.Add(new OptionBorderViewModel(this.QuestionState, false));
                return result;
            }
        }

        public CovariantObservableCollection<CategoricalMultiOptionViewModel<TOptionValue>> Options { get; set; }

        public Identity Identity { get; private set; }

        private string maxAnswersCountMessage;
        public string MaxAnswersCountMessage
        {
            get => maxAnswersCountMessage;
            set => SetProperty(ref maxAnswersCountMessage, value);
        }

        private bool hasOptions;
        public bool HasOptions
        {
            get => hasOptions;
            set => SetProperty(ref hasOptions, value);
        }

        protected CategoricalMultiViewModelBase(
            IQuestionStateViewModel questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository,
            ILiteEventRegistry eventRegistry,
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
        }
        
        protected abstract void SaveAnsweredOptionsForThrottling(IOrderedEnumerable<CategoricalMultiOptionViewModel<TOptionValue>> answeredViewModels);
        protected abstract TInterviewAnswer[] GetAnsweredOptionsFromInterview(IStatefulInterview interview);
        protected abstract void SetAnswerToOptionViewModel(CategoricalMultiOptionViewModel<TOptionValue> optionViewModel, TInterviewAnswer[] answers);
        protected abstract AnswerQuestionCommand GetAnswerCommand(Guid interviewId, Guid userId);
        protected abstract IEnumerable<CategoricalMultiOptionViewModel<TOptionValue>> GetOptions(IStatefulInterview interview);
        protected abstract bool IsInterviewAnswer(TInterviewAnswer interviewAnswer, TOptionValue optionValue);
        protected virtual void Init(IStatefulInterview interview, IQuestionnaire questionnaire) { }
        protected virtual TInterviewAnswer[] FilterAnsweredOptions(TInterviewAnswer[] answeredOptions) => answeredOptions;

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

            this.Init(interview, questionnaire);

            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(entityIdentity.Id);
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(entityIdentity.Id);

            this.throttlingModel.Init(SaveAnswer);

            this.eventRegistry.Subscribe(this, interviewId);

            this.UpdateViewModels();
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
                    this.UpdateOptionsFromInterview();
                }

                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        protected virtual void UpdateViewModels()
        {
            var interview = this.interviewRepository.Get(this.interviewId.FormatGuid());

            this.Options.SynchronizeWith(this.GetOptions(interview).ToList(), (s, t) => s.Value.Equals(t.Value) && s.Title == t.Title);

            this.UpdateOptionsFromInterview();
        }

        protected void InitViewModel(string title, TOptionValue value, IStatefulInterview interview,
            CategoricalMultiOptionViewModel<TOptionValue> vm, bool isAnswerProtected = false)
            => vm.Init(this.QuestionState, title, value, isAnswerProtected, async () => await this.ToggleAnswerAsync(vm));

        private async Task ToggleAnswerAsync(CategoricalMultiOptionViewModel<TOptionValue> optionViewModel)
        {
            var allSelectedOptions = this.Options.Where(x => x.IsSelected()).ToArray();

            if (this.areAnswersOrdered && optionViewModel.IsSelected())
                optionViewModel.CheckedOrder = allSelectedOptions.Count(x => x.IsSelected());

            this.SaveAnsweredOptionsForThrottling(allSelectedOptions.OrderBy(x => x.CheckedOrder));

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        private void UpdateOptionsFromInterview()
        {
            var interview = this.interviewRepository.Get(this.interviewId.FormatGuid());

            var answeredOptions = this.GetAnsweredOptionsFromInterview(interview);

            this.UpdateViewModelsByAnsweredOptions(answeredOptions);
        }

        protected void UpdateViewModelsByAnsweredOptions(TInterviewAnswer[] answeredOptions)
        {
            answeredOptions = answeredOptions ?? Array.Empty<TInterviewAnswer>();

            var filteredAnswers = this.FilterAnsweredOptions(answeredOptions).ToList();
            
            foreach (var option in this.Options)
            {
                var answeredOption = answeredOptions.Where(x => this.IsInterviewAnswer(x, option.Value)).Take(1).ToArray();

                this.SetAnswerToOptionViewModel(option, answeredOption);

                if (this.areAnswersOrdered)
                    option.CheckedOrder = option.Checked && answeredOption.Any() ? filteredAnswers.IndexOf(answeredOption[0]) + 1 : (int?)null;

                if (this.maxAllowedAnswers.HasValue)
                    option.CanBeChecked = option.Checked || filteredAnswers.Count < this.maxAllowedAnswers;
            }

            if (this.maxAllowedAnswers.HasValue)
            {
                this.MaxAnswersCountMessage = string.Format(UIResources.Interview_MaxAnswersCount,
                    filteredAnswers.Count, Math.Min(this.maxAllowedAnswers.Value, this.Options.Count));
            }

            this.HasOptions = this.Options.Any();
        }

        public virtual void Handle(AnswersRemoved @event)
        {
            if (!@event.Questions.Any(x => x.Id == this.Identity.Id && x.RosterVector.Identical(this.Identity.RosterVector))) return;

            this.UpdateOptionsFromInterview();
        }

        public virtual void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);

            this.throttlingModel.Dispose();
            this.QuestionState.Dispose();
        }
    }
}
