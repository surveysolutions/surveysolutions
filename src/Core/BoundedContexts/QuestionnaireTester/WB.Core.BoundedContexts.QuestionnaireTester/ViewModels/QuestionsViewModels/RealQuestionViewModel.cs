using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RealQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel
    {
        private readonly IPrincipal principal;
        private readonly IStatefullInterviewRepository interviewRepository;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<NumericRealQuestionAnswered> QuestionState { get; private set; }
        public SendAnswerViewModel SendAnswerViewModel { get; private set; }

        private decimal? answer;
        public decimal? Answer
        {
            get { return answer; }
            private set
            {
                if (answer != value)
                {
                    answer = value; 
                    RaisePropertyChanged();

                    SendAnswerRealQuestionCommand();
                }
            }
        }

        public RealQuestionViewModel(
            IPrincipal principal,
            IStatefullInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericRealQuestionAnswered> questionStateViewModel,
            SendAnswerViewModel sendAnswerViewModel)
        {
            this.principal = principal;
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

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetRealNumericAnswer(entityIdentity);

            this.Answer = Monads.Maybe(() => answerModel.Answer);
        }

        private async void SendAnswerRealQuestionCommand()
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
        }
    }
}