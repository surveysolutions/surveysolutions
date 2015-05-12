using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class IntegerQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventBusEventHandler<NumericIntegerQuestionAnswered>,
        IDisposable
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private Identity entityIdentity;
        private string interviewId;

        public QuestionHeaderViewModel Header { get; private set; }
        public ValidityViewModel Validity { get; private set; }
        public EnablementViewModel Enablement { get; private set; }

        public IntegerQuestionViewModel(
            ILiteEventRegistry liteEventRegistry,
            ICommandService commandService, 
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionHeaderViewModel questionHeaderViewModel,
            ValidityViewModel validity,
            EnablementViewModel enablement)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Header = questionHeaderViewModel;
            this.Validity = validity;
            this.Enablement = enablement;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.entityIdentity = entityIdentity;
            this.interviewId = interviewId;

            liteEventRegistry.Subscribe(this);

            this.Header.Init(interviewId, entityIdentity);
            this.Validity.Init(interviewId, entityIdentity);
            this.Enablement.Init(interviewId, entityIdentity);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = (IntegerNumericQuestionModel)questionnaire.Questions[entityIdentity.Id];
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);

            this.Answer = Monads.Maybe(() => answerModel.Answer);
            this.MaxValue = Monads.Maybe(() => questionModel.MaxValue);
        }

        private int? maxValue;
        public int? MaxValue
        {
            get { return this.maxValue; }
            private set { this.maxValue = value; RaisePropertyChanged(); }
        }

        private int? answer;
        public int? Answer
        {
            get { return answer; }
            private set { answer = value; RaisePropertyChanged(); }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(SendAnswerTextQuestionCommand)); }
        }

        private void SendAnswerTextQuestionCommand()
        {
            if (!Answer.HasValue) return;

            try
            {
                commandService.Execute(new AnswerNumericIntegerQuestionCommand(
                    interviewId: Guid.Parse(interviewId),
                    userId: principal.CurrentUserIdentity.UserId,
                    questionId: this.entityIdentity.Id,
                    rosterVector: this.entityIdentity.RosterVector,
                    answerTime: DateTime.UtcNow,
                    answer: Answer.Value
                    ));
            }
            catch (Exception)
            {
                Validity.MarkAsError();
            }
        }

        public void Handle(NumericIntegerQuestionAnswered @event)
        {
            if (@event.QuestionId != entityIdentity.Id || @event.PropagationVector.SequenceEqual(entityIdentity.RosterVector))
                return;

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);
            if (answerModel != null)
            {
                this.Answer = answerModel.Answer;
            }
        }

        public void Dispose()
        {
            liteEventRegistry.Unsubscribe(this);
        }
    }
}