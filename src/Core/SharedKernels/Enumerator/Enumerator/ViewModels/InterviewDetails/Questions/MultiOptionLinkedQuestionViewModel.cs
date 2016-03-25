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
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public abstract class MultiOptionLinkedQuestionViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IInterviewEntityViewModel,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IPlainQuestionnaireRepository questionnaireStorage;
        private readonly IPrincipal userIdentity;
        protected readonly ILiteEventRegistry eventRegistry;
        protected readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        protected int? maxAllowedAnswers;
        protected Guid interviewId;
        protected IStatefulInterview interview;
        protected Guid userId;
        protected Identity questionIdentity;
        protected bool areAnswersOrdered;
        private ObservableCollection<MultiOptionLinkedQuestionOptionViewModel> options;
        public QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> QuestionState { get; protected set; }
        public AnsweringViewModel Answering { get; protected set; }

        protected MultiOptionLinkedQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            IStatefulInterviewRepository interviewRepository,
            IPlainQuestionnaireRepository questionnaireStorage,
            IPrincipal userIdentity,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.userIdentity = userIdentity;
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.QuestionState = questionState;
            this.Answering = answering;
            this.Options = new ObservableCollection<MultiOptionLinkedQuestionOptionViewModel>();
        }

        public Identity Identity => this.questionIdentity;

        public void InitAsync(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);
            this.eventRegistry.Subscribe(this, interviewId);
            this.questionIdentity = entityIdentity;
            this.userId = this.userIdentity.CurrentUserIdentity.UserId;
            this.interview = this.interviewRepository.Get(interviewId);
            this.interviewId = this.interview.Id;
            this.InitFromModel(this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity));
            this.Options = new ObservableCollection<MultiOptionLinkedQuestionOptionViewModel>(this.CreateOptions());
        }
        protected abstract void InitFromModel(IQuestionnaire questionnaire);
        protected abstract IEnumerable<MultiOptionLinkedQuestionOptionViewModel> CreateOptions();

        public virtual void Dispose()
        {
            this.eventRegistry.Unsubscribe(this, this.interviewId.FormatGuid());
            this.QuestionState.Dispose();
        }

        public ObservableCollection<MultiOptionLinkedQuestionOptionViewModel> Options
        {
            get { return this.options; }
            private set { this.options = value; this.RaisePropertyChanged(() => this.HasOptions); }
        }

        public bool HasOptions => this.Options.Any();

        public async Task ToggleAnswerAsync(MultiOptionLinkedQuestionOptionViewModel changedModel)
        {
            List<MultiOptionLinkedQuestionOptionViewModel> allSelectedOptions =
                this.areAnswersOrdered ?
                    this.Options.Where(x => x.Checked).OrderBy(x => x.CheckedTimeStamp).ThenBy(x => x.CheckedOrder).ToList() :
                    this.Options.Where(x => x.Checked).ToList();

            if (this.maxAllowedAnswers.HasValue && allSelectedOptions.Count > this.maxAllowedAnswers)
            {
                changedModel.Checked = false;
                return;
            }

            decimal[][] selectedValues = allSelectedOptions
                .Select(x => x.Value)
                .ToArray();

            var command = new AnswerMultipleOptionsLinkedQuestionCommand(
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

        private void PutOrderOnOptions(MultipleOptionsLinkedQuestionAnswered @event)
        {
            foreach (var option in this.Options)
            {
                var foundIndex = @event.SelectedRosterVectors
                    .Select((o, i) => new { index = i, selectedVector = o })
                    .FirstOrDefault(item => item.selectedVector.Identical(option.Value));

                var selectedOptionIndex = foundIndex?.index ?? -1;

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

        public void Handle(MultipleOptionsLinkedQuestionAnswered @event)
        {
            if (this.areAnswersOrdered && @event.QuestionId == this.questionIdentity.Id && @event.RosterVector.Identical(this.questionIdentity.RosterVector))
            {
                this.PutOrderOnOptions(@event);
            }
        }
    }
}