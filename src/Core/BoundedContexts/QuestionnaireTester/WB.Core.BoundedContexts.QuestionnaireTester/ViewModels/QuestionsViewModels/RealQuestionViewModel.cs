using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RealQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventHandler<NumericRealQuestionAnswered>,
        IDisposable
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<NumericRealQuestionAnswered> QuestionState { get; private set; }
        public SendAnswerViewModel SendAnswerViewModel { get; private set; }

        public RealQuestionViewModel(
            ILiteEventRegistry liteEventRegistry,
            ICommandService commandService, 
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericRealQuestionAnswered> questionStateViewModel,
            SendAnswerViewModel sendAnswerViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.SendAnswerViewModel = sendAnswerViewModel;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            liteEventRegistry.Subscribe(this);

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

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

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(SendAnswerTextQuestionCommand)); }
        }

        private async void SendAnswerTextQuestionCommand()
        {
            if (!Answer.HasValue) return;

            var command = new AnswerNumericRealQuestionCommand(
                interviewId: Guid.Parse(interviewId),
                userId: principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: Answer.Value);

            try
            {
                await SendAnswerViewModel.SendAnswerQuestionCommand(command);
                QuestionState.ExecutedAnswerCommandWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                QuestionState.ProcessAnswerCommandException(ex);
            }
        }

        public void Handle(NumericRealQuestionAnswered @event)
        {
            if (@event.QuestionId != questionIdentity.Id || !@event.PropagationVector.SequenceEqual(questionIdentity.RosterVector))
                return;

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetRealNumericAnswer(questionIdentity);
            this.Answer = Monads.Maybe(() => answerModel.Answer);
        }

        public void Dispose()
        {
            liteEventRegistry.Unsubscribe(this);
        }
    }
}