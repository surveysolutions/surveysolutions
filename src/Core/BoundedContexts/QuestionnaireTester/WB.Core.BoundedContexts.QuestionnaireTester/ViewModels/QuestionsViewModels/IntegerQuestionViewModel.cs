using System;
using System.Linq;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class IntegerQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel
    {
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IUserInteraction userInteraction;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<NumericIntegerQuestionAnswered> QuestionState { get; private set; }
        public SendAnswerViewModel SendAnswerViewModel { get; private set; }

        private bool isRosterSizeQuestion;

        private int? previousAnswer;

        private int? answer;
        public int? Answer
        {
            get { return answer; }
            private set
            {
                if (answer != value)
                {
                    answer = value;
                    RaisePropertyChanged();

                    this.SendAnswerIntegerQuestionCommand();
                }
            }
        }

        public IntegerQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericIntegerQuestionAnswered> questionStateViewModel,
            IUserInteraction userInteraction,
            SendAnswerViewModel sendAnswerViewModel)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.userInteraction = userInteraction;
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
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = (IntegerNumericQuestionModel)questionnaire.Questions[entityIdentity.Id];

            this.Answer = Monads.Maybe(() => answerModel.Answer);
            this.previousAnswer = Monads.Maybe(() => answerModel.Answer);
            this.isRosterSizeQuestion = questionModel.IsRosterSizeQuestion;
        }

        private async void SendAnswerIntegerQuestionCommand()
        {
            if (!Answer.HasValue) return;

            if (isRosterSizeQuestion && previousAnswer.HasValue && Answer < previousAnswer)
            {
                var amountOfRostersToRemove = previousAnswer - Math.Max(Answer.Value, 0);
                var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                if (!(await userInteraction.ConfirmAsync(message)))
                {
                    Answer = previousAnswer;
                    return;
                }
            }

            var command = new AnswerNumericIntegerQuestionCommand(
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

                previousAnswer = Answer;
            }
            catch (InterviewException ex)
            {
                QuestionState.ProcessAnswerCommandException(ex);
            }
        }
    }
}