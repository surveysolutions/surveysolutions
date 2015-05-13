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

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RealQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventBusEventHandler<NumericRealQuestionAnswered>,
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
        public CommentsViewModel Comments { get; private set; }

        public RealQuestionViewModel(
            ILiteEventRegistry liteEventRegistry,
            ICommandService commandService, 
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionHeaderViewModel questionHeaderViewModel,
            ValidityViewModel validity,
            EnablementViewModel enablement,
            CommentsViewModel comments)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Header = questionHeaderViewModel;
            this.Validity = validity;
            this.Enablement = enablement;
            this.Comments = comments;
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
            this.Comments.Init(interviewId, entityIdentity);
            this.Enablement.Init(interviewId, entityIdentity);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetRealNumericAnswer(entityIdentity);

            this.Answer = Monads.Maybe(() => answerModel.Answer);
        }

        private decimal? answer;
        public decimal? Answer
        {
            get { return answer; }
            private set { answer = value; RaisePropertyChanged(); }
        }

        private bool isAnswered;
        public bool IsAnswered
        {
            get { return isAnswered; }
            set { isAnswered = value; RaisePropertyChanged(); }
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
                commandService.Execute(new AnswerNumericRealQuestionCommand(
                    interviewId: Guid.Parse(interviewId),
                    userId: principal.CurrentUserIdentity.UserId,
                    questionId: this.entityIdentity.Id,
                    rosterVector: this.entityIdentity.RosterVector,
                    answerTime: DateTime.UtcNow,
                    answer: Answer.Value
                    ));
                Validity.ExecutedWithoutExceptions();
            }
            catch (Exception ex)
            {
                Validity.ProcessException(ex);
            }
        }

        public void Handle(NumericRealQuestionAnswered @event)
        {
            if (@event.QuestionId != entityIdentity.Id || @event.PropagationVector.SequenceEqual(entityIdentity.RosterVector))
                return;

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetRealNumericAnswer(entityIdentity);
            this.Answer = Monads.Maybe(() => answerModel.Answer);
            this.IsAnswered = interview.WasAnswered(entityIdentity);
        }

        public void Dispose()
        {
            liteEventRegistry.Unsubscribe(this);
        }
    }
}